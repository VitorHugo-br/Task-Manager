using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using Task_Manager.Data;
using Task_Manager.Extensions;
using Task_Manager.Models;
using Task_Manager.Models.DTO;
using Task_Manager.Models.Enums;
using Task_Manager.Services;

namespace Task_Manager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class MyTasksController(TaskDbContext context, AuthService authService) : ControllerBase
    {
        private readonly TaskDbContext _context = context;
        private readonly AuthService _authService = authService;

        [HttpGet]
        [Route("GetTasks")]
        public async Task<ActionResult<IEnumerable<MyTask>>> GetTasks()
        {
            var tasks = await _context.Tasks.ToListAsync();
            return Ok(tasks);
        }

        [HttpGet]
        [Route("GetTasksFiltered")]
        public async Task<ActionResult<IEnumerable<MyTask>>> GetTasks([FromQuery] FilterTasksDto filterTasksDto)
        {

            var tasksQuery = _context.Tasks.AsQueryable();
            tasksQuery = AddFilters(filterTasksDto, tasksQuery);
            var filteredTasks = await tasksQuery.ToListAsync();
            return Ok(filteredTasks);

        }

        private static IQueryable<MyTask> AddFilters(FilterTasksDto filter, IQueryable<MyTask> tasksQuery)
        {
            return tasksQuery
                .WhereIf(filter.TaskId.HasValue, t => t.Id == filter.TaskId!.Value)
                .WhereIf(filter.UserId.HasValue, t => t.UserId == filter.UserId!.Value)
                .WhereIf(filter.IssuerId.HasValue, t => t.IssuerId == filter.IssuerId!.Value)
                .WhereIf(filter.Status.HasValue, t => t.Status == filter.Status!.Value)
                .WhereIf(filter.CreationDate.HasValue, t => t.CreatedAt.HasValue && t.CreatedAt.Value.Date == filter.CreationDate!.Value.Date)
                .WhereIf(filter.DueDate.HasValue, t => t.DueDate.HasValue && t.DueDate.Value.Date == filter.DueDate!.Value.Date);
        }

        [HttpPost]
        [Route("CreateTask")]
        public async Task<ActionResult<MyTask>> CreateTask([FromBody] TaskDto task)
        {

            var bearerToken = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(bearerToken);
            var userEmail = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user == null) return NotFound("User not found");

            var newTask = new MyTask
            {
                Title = task.Title,
                Guid = Guid.NewGuid(),
                Description = task.Description,
                Status = (Status)task.Status,
                StartDate = task.StartDate,
                EndDate = task.EndDate,
                DueDate = task.DueDate,
                IssuerId = user.Id,
                UserId = task.UserId
            };

            _context.Tasks.Add(newTask);
            await _context.SaveChangesAsync();
            return Created();
        }

        [HttpPatch]
        [Route("UpdateTask/{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskDto task)
        {
            if (id == 0) return BadRequest("Id must be valid");

            var existingTask = await _context.Tasks.FindAsync(id);
            if (existingTask == null) return NotFound("Task not found");

            if (task.Title is not null) existingTask.Title = task.Title;
            if (task.Description is not null) existingTask.Description = task.Description;
            if (task.Status != existingTask.Status) existingTask.Status = task.Status;
            if (task.StartDate is not null) existingTask.StartDate = task.StartDate;
            if (task.EndDate is not null) existingTask.EndDate = task.EndDate;
            if (task.DueDate is not null) existingTask.DueDate = task.DueDate;
            if (task.UserId is not null) existingTask.UserId = task.UserId;

            _context.Entry(existingTask).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Created();

        }

    }
}
