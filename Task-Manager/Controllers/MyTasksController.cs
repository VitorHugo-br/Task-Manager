using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using Task_Manager.Data;
using Task_Manager.Models;
using Task_Manager.Models.DTO;
using Task_Manager.Services;

namespace Task_Manager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class MyTasksController(TaskDbContext context, AuthService authService) : Controller
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
        [Route("GetTaskById/{id}")]
        public async Task<ActionResult<MyTask>> GetTaskById(int id)
        {

            var task = await _context.Tasks.FindAsync(id);

            if (task == null)
            {
                return NotFound("Task not found!");
            }

            return Ok(task);
        }

        [HttpPost]
        [Route("CreateTask")]
        public async Task<ActionResult<MyTask>> CreateTask([FromBody] TaskDTO task)
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
                RequestedBy = user.Name,
                UserId = task.UserId
            };

            _context.Tasks.Add(newTask);
            await _context.SaveChangesAsync();
            return Created();
        }

        //TODO: Modificar o update para receber apenas os campos que podem ser atualizados, e não o objeto inteiro. e possibitar o update parcial, ou seja, atualizar apenas os campos que foram enviados no request.
        [HttpPut]
        [Route("UpdateTask/{id}")]
        public async Task<IActionResult> UpdateTask(int id, TaskDTO task)
        {
            //Verificar se o token é válido
            //var bearerToken = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            //var isValidToken = _authService.ValidateToken(bearerToken);
            //if (!isValidToken) return Unauthorized("Invalid Credentials");

            //Verificar se a task existe
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
