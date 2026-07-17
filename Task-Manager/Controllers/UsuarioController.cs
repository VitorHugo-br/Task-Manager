using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Task_Manager.Data;
using Task_Manager.Models;
using Task_Manager.Models.DTO;
using Task_Manager.Services;

namespace Task_Manager.Controllers;

[ApiController]
[Route("[controller]")]
public class UsuarioController(TaskDbContext context, RedisService redisService) : ControllerBase
{
    private readonly IDatabase _redis = redisService.GetDatabase();

    [HttpGet]
    [Route("listar-usuarios")]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarUsuarios()
    {
        const string cacheKey = "users";
        var cached = _redis.StringGet(cacheKey);
        if (cached.HasValue)
        {
            var cachedUsers = JsonSerializer.Deserialize<List<UsuarioDto>>(cached.ToString());
            return Ok(cachedUsers);
        }

        var users = await context.Usuarios
                                 .AsNoTracking()
                                 .Select(u => new UsuarioDto(u.Id, u.Nome))
                                 .ToListAsync();

        _redis.StringSet(cacheKey, JsonSerializer.Serialize(users), TimeSpan.FromMinutes(5));

        return Ok(users);
    }

    [HttpGet]
    [Route("listIssuers")]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    public IActionResult ListIssuers()
    {
        const string cacheKey = "issuers";
        var cached = _redis.StringGet(cacheKey);
        if (cached.HasValue)
        {
            var cachedIssuers = JsonSerializer.Deserialize<List<Usuario>>(cached.ToString());
            return Ok(cachedIssuers);
        }

        var tasks = context.Chamados.ToList();
        var taskIssuers = tasks.Select(tk => tk.RemetenteId).ToHashSet();
        var issuers = context.Usuarios.Where(u => taskIssuers.Contains(u.Id)).ToList();
        _redis.StringSet(cacheKey, JsonSerializer.Serialize(issuers), TimeSpan.FromMinutes(5));
        return Ok(issuers);
    }
}