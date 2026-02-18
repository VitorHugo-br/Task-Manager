using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task_Manager.Data;
using Task_Manager.Models;

namespace Task_Manager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MyTasksController : Controller
    {
        private readonly TaskDbContext _context;

        public MyTasksController(TaskDbContext context)
        {
            _context = context;
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
        public async Task<ActionResult<MyTask>> CreateTask(MyTask task)
        {
            _context.Tasks.Add(task);
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
