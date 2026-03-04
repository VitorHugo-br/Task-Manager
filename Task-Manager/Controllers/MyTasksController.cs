using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;
using Task_Manager.Data;
using Task_Manager.Extensions;
using Task_Manager.Helpers;
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
    public async Task<ActionResult<IEnumerable<MyTask>>> GetTasks()
    {
        var tasksFromRedis = _redis.JSON().Get("tasks");

        if (!tasksFromRedis.IsNull)
        {
            var deserializedTasks = JsonConvert.DeserializeObject<List<MyTask>>(tasksFromRedis.ToString());
            return Ok(value: deserializedTasks);
        }

        var tasks = await context
            .Tasks
            .AsNoTracking()
            .ToListAsync();

        _redis.JSON().Set("tasks", "$", tasks);
        _redis.KeyExpire("tasks", TimeSpan.FromMinutes(10));

        return Ok(tasks);
    }

    [HttpGet]
    [Route("GetTasksFiltered")]
    public async Task<ActionResult<IEnumerable<MyTask>>> GetTasks([FromQuery] FilterTasksDto filterTasksDto)
    {
        var cacheKey = CacheKeyHelper.BuildFilterKey("tasks", filterTasksDto);

        var cached = _redis.JSON().Get(cacheKey);
        if (!cached.IsNull)
        {
            var cachedTasks = JsonConvert.DeserializeObject<List<MyTask>>(cached.ToString());
            return Ok(cachedTasks);
        }

        var tasksQuery = context.Tasks.AsQueryable();
        tasksQuery = AddFilters(filterTasksDto, tasksQuery);
        var filteredTasks = await tasksQuery.ToListAsync();
        
        _redis.JSON().Set(cacheKey, "$", filteredTasks);
        _redis.KeyExpire(cacheKey, TimeSpan.FromMinutes(5));
        
        return Ok(filteredTasks);
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
        var bearerToken = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(bearerToken);
        var userEmail = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

        if (user == null) return NotFound("User not found");

        var newTask = new MyTask
        {
            Title = task.Title,
            Guid = Guid.NewGuid(),
            Description = task.Description,
            Status = task.Status,
            StartDate = task.StartDate,
            EndDate = task.EndDate,
            DueDate = task.DueDate,
            IssuerId = user.Id,
            UserId = task.UserId
        };

        context.Tasks.Add(newTask);
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
        return Created();
    }
}