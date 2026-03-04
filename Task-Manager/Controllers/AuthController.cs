using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task_Manager.Data;
using Task_Manager.Models;
using Task_Manager.Models.DTO;
using Task_Manager.Services;

namespace Task_Manager.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(TaskDbContext context, AuthService authService) : ControllerBase
{
    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Create([FromBody] UserDto user)
    {
        var existingUser = await context.Users.AnyAsync(u => u.Email == user.Email);
        if (existingUser) return BadRequest("User already exists");

        var newUser = new User
        {
            Name = user.Name,
            Email = user.Email,
            Password = authService.GetHashedPassword(user.Password),
            Role = user.Role
        };

        await context.Users.AddAsync(newUser);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(Create), new { id = newUser.Id }, new { newUser.Id });
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto login)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == login.Email);
        if (user == null || !authService.VerifyPassword(login.Password, user.Password))
        {
            return Unauthorized("Invalid credentials");
        }

        var token = authService.GenerateToken(user);

        return Ok(token);
    }
}