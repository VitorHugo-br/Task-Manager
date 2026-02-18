using Microsoft.AspNetCore.Mvc;
using Task_Manager.Data;
using Task_Manager.Models;
using Task_Manager.Models.DTO;
using Task_Manager.Services;

namespace Task_Manager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : Controller
    {

        private readonly TaskDbContext _context;
        private readonly AuthService _authService;

        public UserController(TaskDbContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Create(UserDTO user)
        {

            if (user == null)
            {
                return BadRequest("Invalid data");
            }

            if (_context.Users.Any(u => u.Email == user.Email))
            {
                return BadRequest("User already exists");
            }

            var newUser = new User
            {
                Name = user.Name,
                Email = user.Email,
                Password = _authService.GetHashedPassword(user.Password),
                Roles = user.Roles
            };

            var userToken = _authService.GenerateToken(newUser);

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            return Created(String.Empty, userToken);
        }
    }
}
