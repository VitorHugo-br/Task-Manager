using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task_Manager.Data;
using Task_Manager.Helpers;
using Task_Manager.Interfaces;
using Task_Manager.Models;
using Task_Manager.Models.DTO;
using Task_Manager.Services;

namespace Task_Manager.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(
    TaskDbContext context,
    AuthService authService,
    ILogService logService,
    AuditService auditService
) : ControllerBase
{
    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Create([FromBody] UserDto user)
    {
        if (!EmailVerification.IsValid(user.Email)) return BadRequest("Invalid email format");

        var existingUser = await context.Users.AnyAsync(u => u.Email == user.Email);
        if (existingUser) return BadRequest("User already exists");

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        return Created("User created successfully", user);
    }

    [HttpPost]
    [Route("register-bulk")]
    public async Task<IActionResult> CreateBulk([FromBody] IEnumerable<UserDto> users)
    {
        var userList = users.ToList();

        var emailsValidos = userList
            .Where(u => EmailVerification.IsValid(u.Email))
            .ToList();

        if (emailsValidos.Count == 0)
            return BadRequest("Nenhum e-mail válido informado.");

        var emailsRecebidos = emailsValidos
            .Select(u => u.Email)
            .ToList();

        // ✅ Busca emails existentes em memória — evita o bug do provider MySQL
        var emailsExistentes = await context.Users
            .AsNoTracking()
            .Select(u => u.Email)
            .ToHashSetAsync();

        var novosUsuarios = emailsValidos
            .Where(dto => !emailsExistentes.Contains(dto.Email))
            .Select(dto => (User)dto)
            .ToList();

        if (novosUsuarios.Count == 0)
            return BadRequest("Nenhum usuário válido para cadastrar.");

        await context.Users.AddRangeAsync(novosUsuarios);
        await context.SaveChangesAsync();

        return Ok($"{novosUsuarios.Count} usuários criados com sucesso.");
    }


    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto login)
    {
        try
        {
            if (!EmailVerification.IsValid(login.Email)) return BadRequest("Invalid email format or empty email");

            var user = await CompiledQueries.GetUserByEmail(context, login.Email);
            if (user == null || !authService.VerifyPassword(login.Password, user.Password))
            {
                return Unauthorized("Invalid credentials");
            }

            var token = authService.GenerateToken(user);

            var auditLog = new AuditLogDto(
                "Login",
                "User",
                user.Id,
                user.Id,
                null,
                null,
                ExtractIpAddress()
            );

            await auditService.Log(auditLog);

            return Ok(new { token });
        }
        catch (Exception ex)
        {
            await logService.Error(ex.Message, ex, nameof(Login));
            return StatusCode(Convert.ToInt32(HttpStatusCode.InternalServerError), "Internal Server Error");
        }
    }

    private string? ExtractIpAddress()
    {
        var ip = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(ip)) return ip;
        var remoteIp = HttpContext.Connection.RemoteIpAddress;

        if (remoteIp != null)
        {
            ip = remoteIp.IsIPv4MappedToIPv6
                ? remoteIp.MapToIPv4().ToString()
                : remoteIp.ToString();
        }

        return ip;
    }
}