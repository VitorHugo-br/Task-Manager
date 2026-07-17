using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Text;
using Task_Manager.Data;
using Task_Manager.Helpers;
using Task_Manager.Interfaces;
using Task_Manager.Models.DTO;
using Task_Manager.Services;

namespace Task_Manager.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(
    TaskDbContext context,
    AuthService authService,
    ILogService logService,
    AuditService auditService,
    JsonWebTokenHandler tokenValidator,
    IConfiguration configuration
) : ControllerBase
{
    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Create([FromBody] RegisterDto user)
    {
        if (!EmailVerification.IsValid(user.Email)) return BadRequest("Invalid email format");

        var existingUser = await context.Usuarios.AnyAsync(u => u.Email == user.Email);
        if (existingUser) return BadRequest("User already exists");

        await context.Usuarios.AddAsync(user);
        await context.SaveChangesAsync();

        return Created("User created successfully", user);
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto login)
    {
        try
        {
            if (!EmailVerification.IsValid(login.Email)) return BadRequest("Invalid email format or empty email");

            var user = await CompiledQueries.GetUserByEmail(context, login.Email);
            if (user == null || !authService.VerifyPassword(login.Password, user.Senha))
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

    [HttpPost]
    [Route("validar-token")]
    public async Task<IActionResult> ValidarTokenAsync(ValidarTokenDto dto)
    {
        if (string.IsNullOrEmpty(dto.token)) return Unauthorized(false);

        var key = configuration["SecretKey"];

        if (key == null) return Unauthorized(false);

        var keyEncoded = Encoding.ASCII.GetBytes(key);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyEncoded),
            ValidIssuer = "TaskManager-Api",
            ValidateIssuer = true,
            ValidAudience = "TaskManager-Front",
            ValidateAudience = true,
            ClockSkew = TimeSpan.Zero
        };

        var result = await tokenValidator.ValidateTokenAsync(dto.token, validationParameters);

        return Ok(result.IsValid);

    }

}