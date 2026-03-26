using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;
using Task_Manager.Data;
using Task_Manager.Extensions;
using Task_Manager.Models;
using Task_Manager.Models.DTO;
using Task_Manager.Services;

namespace Task_Manager.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class MyTasksController(TaskDbContext context, RedisService redisService) : ControllerBase
{
    private readonly IDatabase _redis = redisService.GetDatabase();

    [HttpGet]
    [Route("GetTasks")]
    public async Task<ActionResult<PagedResponse<MyTask>>> GetTasks(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10
    )
    {
        if (page < 1 || pageSize < 1)
            return BadRequest("Page e PageSize devem ser maiores que zero.");

        if (pageSize > 50)
            return BadRequest("PageSize máximo é 50.");

        var cacheKey = $"tasks:page={page}:size={pageSize}";

        var cached = _redis.JSON().Get(cacheKey);
        if (!cached.IsNull)
        {
            var cachedResponse = JsonSerializer.Deserialize<PagedResponse<MyTask>>(cached.ToString());
            return Ok(cachedResponse);
        }

        
        var query = context.Tasks.AsNoTracking();

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var response = new PagedResponse<MyTask>(
            Items: items,
            Page: page,
            PageSize: pageSize,
            TotalItems: totalItems,
            TotalPages: totalPages
        );

        // 3. Cacheia a página específica
        _redis.JSON().Set(cacheKey, "$", response);
        _redis.KeyExpire(cacheKey, TimeSpan.FromMinutes(10));

        return Ok(response);
    }

    [HttpGet]
    [Route("GetTasksFiltered")]
    public async Task<ActionResult<IEnumerable<MyTask>>> GetTasks([FromQuery] FilterTasksDto filterTasksDto)
    {
        // 1. Gera uma chave única baseada nos filtros aplicados
        var cacheKey = BuildCacheKey(filterTasksDto);

        // 2. Tenta buscar do cache
        var cached = _redis.JSON().Get(cacheKey);

        if (!cached.IsNull)
        {
            var deserializedTasks = JsonSerializer.Deserialize<List<MyTask>>(cached.ToString());
            return Ok(deserializedTasks);
        }

        // 3. Cache miss — busca no banco com os filtros
        var tasksQuery = context.Tasks.AsQueryable();
        tasksQuery = AddFilters(filterTasksDto, tasksQuery);
        var filteredTasks = await tasksQuery.ToListAsync();

        // 4. Armazena no cache com a chave específica dos filtros
        _redis.JSON().Set(cacheKey, "$", filteredTasks);
        _redis.KeyExpire(cacheKey, TimeSpan.FromMinutes(5));

        return Ok(filteredTasks);
    }

    private static string BuildCacheKey(FilterTasksDto filter)
    {
        
        var parts = new List<string>();

        if (filter.TaskId.HasValue) parts.Add($"taskId={filter.TaskId}");
        if (filter.UserId.HasValue) parts.Add($"userId={filter.UserId}");
        if (filter.IssuerId.HasValue) parts.Add($"issuerId={filter.IssuerId}");
        if (filter.Status.HasValue) parts.Add($"status={(int)filter.Status}");
        if (filter.CreationDate.HasValue) parts.Add($"creationDate={filter.CreationDate:yyyy-MM-dd}");
        if (filter.DueDate.HasValue) parts.Add($"dueDate={filter.DueDate:yyyy-MM-dd}");

        var suffix = parts.Count > 0
            ? string.Join(":", parts)
            : "all";

        return $"tasks-filtered:{suffix}";
    }

    private static IQueryable<MyTask> AddFilters(FilterTasksDto filter, IQueryable<MyTask> tasksQuery)
    {
        return tasksQuery
            .WhereIf(filter.TaskId.HasValue, t => t.Id == filter.TaskId!.Value)
            .WhereIf(filter.UserId.HasValue, t => t.UserId == filter.UserId!.Value)
            .WhereIf(filter.IssuerId.HasValue, t => t.IssuerId == filter.IssuerId!.Value)
            .WhereIf(filter.Status.HasValue, t => t.Status == filter.Status!.Value)
            .WhereIf(filter.CreationDate.HasValue,
                t => t.CreatedAt.HasValue && t.CreatedAt.Value.Date == filter.CreationDate!.Value.Date)
            .WhereIf(filter.DueDate.HasValue,
                t => t.DueDate.HasValue && t.DueDate.Value.Date == filter.DueDate!.Value.Date);
    }

    [HttpPost]
    [Route("CreateTask")]
    public async Task<ActionResult<MyTask>> CreateTask([FromBody] TaskDto task)
    {
        var userEmail = HttpContext.User.FindFirstValue(ClaimTypes.Email);

        if (string.IsNullOrEmpty(userEmail)) return Unauthorized("User not authenticated");

        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

        if (user == null) return NotFound("User not found");

        MyTask newTask = task;
        newTask.IssuerId = user.Id;

        await context.Tasks.AddAsync(newTask);
        await context.SaveChangesAsync();

        return Created();
    }

    [HttpPatch]
    [Route("UpdateTask/{id}")]
    public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskDto task)
    {
        if (id == 0) return BadRequest("Id must be valid");

        var existingTask = await context.Tasks.FindAsync(id);
        if (existingTask == null) return NotFound("Task not found");

        existingTask.Title = task.Title;
        existingTask.Description = task.Description;
        if (task.Status != existingTask.Status) existingTask.Status = task.Status;
        if (task.StartDate is not null) existingTask.StartDate = task.StartDate;
        if (task.EndDate is not null) existingTask.EndDate = task.EndDate;
        if (task.DueDate is not null) existingTask.DueDate = task.DueDate;
        if (task.UserId is not null) existingTask.UserId = task.UserId;

        context.Entry(existingTask).State = EntityState.Modified;
        await context.SaveChangesAsync();

        _redis.KeyDelete("tasks");
        redisService.RemoveByPattern("tasks-filtered:*");

        return Created();
    }

    [HttpPost]
    [Route("CreateTaskBulk")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> CreateTaskBulk([FromBody] IEnumerable<TaskDto> tasks)
    {
        var userEmail = HttpContext.User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(userEmail)) return Unauthorized("User not authenticated");

        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
        if (user == null) return NotFound("User not found");

        var taskList = tasks.ToList();
        if (taskList.Count == 0) return BadRequest("Nenhuma task informada.");

        var novasTasks = taskList
            .Select(dto =>
            {
                MyTask task = dto; // implicit operator
                task.IssuerId = user.Id;
                task.Guid = Guid.NewGuid();
                return task;
            })
            .ToList();

        await context.Tasks.AddRangeAsync(novasTasks);
        await context.SaveChangesAsync();

        _redis.KeyDelete("tasks");
        redisService.RemoveByPattern("tasks-filtered:*");

        return Ok($"{novasTasks.Count} tasks criadas com sucesso.");
    }
}