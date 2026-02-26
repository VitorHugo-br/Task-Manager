using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task_Manager.Data;
using Task_Manager.Models;
using Task_Manager.Models.DTO;
using Task_Manager.Services;

namespace Task_Manager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {

        private readonly TaskDbContext _context;
        private readonly AuthService _authService;

        public AuthController(TaskDbContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Create([FromBody] UserDto user)
        {
            if (user == null) return BadRequest("Invalid data");

            if (await _context.Users.AnyAsync(u => u.Email == user.Email)) return BadRequest("User already exists");

            var newUser = new User
            {
                Name = user.Name,
                Email = user.Email,
                Password = _authService.GetHashedPassword(user.Password),
                Role = user.Role
            };

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Create), new { id = newUser.Id }, new { Id = newUser.Id });
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            if (login == null) return BadRequest("Invalid data");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == login.Email);
            if (user == null || !_authService.VerifyPassword(login.Password, user.Password))
            {
                return Unauthorized("Invalid credentials");
            }

            var token = _authService.GenerateToken(user);

            return Ok(new { Token = token });

        }
    }
}
