using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Task_Manager.Data;

namespace Task_Manager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController(TaskDbContext context) : Controller
    {

        private readonly TaskDbContext _context = context;

        [HttpGet]
        [Route("listUsers")]
        public IActionResult ListUsers()
        {
            var users = _context.Users.ToList();
            return Ok(users);
        }

        [HttpGet]
        [Route("listIssuers")]
        public IActionResult ListIssuers()
        {
            var tasks = _context.Tasks.ToList();
            var taskIssuers = tasks.Select(tk => tk.IssuerId).ToHashSet();
            var issuers = _context.Users.Where(u => taskIssuers.Contains(u.Id)).ToList();
            
            return Ok(issuers);
        }
    }
}
