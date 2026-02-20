using Microsoft.AspNetCore.Mvc;
using Task_Manager.Data;
using Task_Manager.Models;
using Task_Manager.Models.DTO;
using Task_Manager.Services;

namespace Task_Manager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController(TaskDbContext context, AuthService authService) : Controller
    {

        private readonly TaskDbContext _context = context;
        private readonly AuthService _authService = authService;

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Create([FromBody] UserDTO user)
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
                Role = user.Role
            };

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            return Created();
        }

        [HttpPost]
        [Route("login")]
        public IActionResult Login([FromBody] LoginDTO login)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == login.Email);
            if (user == null || !_authService.VerifyPassword(login.Password, user.Password))
            {
                return Unauthorized("Invalid credentials");
            }

            var token = _authService.GenerateToken(user);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddHours(1)
            };

            Response.Cookies.Append("tkon", token, cookieOptions);

            return Ok(new { Token = token });

        }
    }
}
