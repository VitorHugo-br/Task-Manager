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
    public class MyTasksController : Controller
    {
        private readonly TaskDbContext _context;
        private readonly AuthService _authService;

        public MyTasksController(TaskDbContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        [HttpGet]
        [Route("GetTasks")]
        public async Task<IEnumerable<MyTask>> GetTasks()
        {
            var tasks = await _context.Tasks.ToListAsync();
            return tasks;
        }

        [HttpGet]
        [Route("GetTaskById/{id}")]
        public async Task<ActionResult<MyTask>> GetTaskById(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }
            return task;
        }

        [HttpPost]
        [Route("CreateTask")]
        public async Task<ActionResult<MyTask>> CreateTask([FromBody] TaskDTO task)
        {

            var bearerToken = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            var isValidToken = _authService.ValidateToken(bearerToken);
            if (!isValidToken) return Unauthorized("Invalid Credentials");

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(bearerToken);
            var userEmail = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user == null) return NotFound("User not found");

            var newTask = new MyTask
            {
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                StartDate = task.StartDate,
                EndDate = task.EndDate,
                DueDate = task.DueDate,
                RequestedBy = user.Name,
                User = await _context.Users.FirstOrDefaultAsync(u => u.Id == task.UserId)
            };

            _context.Tasks.Add(newTask);
            await _context.SaveChangesAsync();
            return Created();
        }

        //TODO: Modificar o update para receber apenas os campos que podem ser atualizados, e não o objeto inteiro. e possibitar o update parcial, ou seja, atualizar apenas os campos que foram enviados no request.
        [HttpPut]
        [Route("UpdateTask/{id}")]
        public async Task<IActionResult> UpdateTask(int id, MyTask updatedTask)
        {
            if (id != updatedTask.Id)
            {
                return BadRequest();
            }
            _context.Entry(updatedTask).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaskExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        private bool TaskExists(int id)
        {
            return _context.Tasks.Any(e => e.Id == id);
        }
    }
}
