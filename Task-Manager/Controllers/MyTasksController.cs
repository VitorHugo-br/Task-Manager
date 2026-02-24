using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using Task_Manager.Data;
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
        public async Task<ActionResult<IEnumerable<MyTask>>> GetTasks([FromQuery] int? TaskId, [FromQuery] int? UserId, [FromQuery] int? IssuerId, [FromQuery] DateTime? CreationDate, [FromQuery] DateTime? DueDate, [FromQuery] int? Status)
        {

            var tasksQuery = _context.Tasks.AsQueryable();

            if (TaskId.HasValue)
                tasksQuery = tasksQuery.Where(t => t.Id == TaskId.Value);
            if (UserId.HasValue)
                tasksQuery = tasksQuery.Where(t => t.UserId == UserId.Value);
            if (IssuerId.HasValue)
                tasksQuery = tasksQuery.Where(t => t.IssuerId == IssuerId.Value);
            if (CreationDate.HasValue)
                tasksQuery = tasksQuery.Where(t => t.CreatedAt.HasValue && t.CreatedAt.Value.Date == CreationDate.Value.Date);
            if (DueDate.HasValue)
                tasksQuery = tasksQuery.Where(t => t.DueDate.HasValue && t.DueDate.Value.Date == DueDate.Value.Date);
            if (Status.HasValue)
                tasksQuery = tasksQuery.Where(t => t.Status == (Status)Status.Value);

            var filteredTasks = await tasksQuery.ToListAsync();
            return Ok(filteredTasks);

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
                Status = task.Status,
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

        //TODO: Modificar o update para receber apenas os campos que podem ser atualizados, e não o objeto inteiro. e possibitar o update parcial, ou seja, atualizar apenas os campos que foram enviados no request.
        [HttpPut]
        [Route("UpdateTask/{id}")]
        public async Task<IActionResult> UpdateTask(int id, TaskDto task)
        {
            if (task == null) return BadRequest("Task data is required.");
            if (!TaskExists(id)) return BadRequest($"TaskID: {id} doesn't exist!");

            var ExistingTask = await _context.Tasks.FirstAsync(tk => tk.Id == id);

            ExistingTask.Title = task.Title;
            ExistingTask.Description = task.Description;
            ExistingTask.Status = task.Status;
            ExistingTask.StartDate = task.StartDate;
            ExistingTask.EndDate = task.EndDate;
            ExistingTask.DueDate = task.DueDate;
            ExistingTask.User = await _context.Users.FirstOrDefaultAsync(u => u.Id == task.UserId);

            _context.Entry(ExistingTask).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok($"Task {ExistingTask.Id} updated");

        }

        private bool TaskExists(int id)
        {
            return _context.Tasks.Any(e => e.Id == id);
        }
    }
}
