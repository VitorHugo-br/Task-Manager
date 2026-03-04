using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Task_Manager.Data;
using Task_Manager.Models;
using Task_Manager.Services;

namespace Task_Manager.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController(TaskDbContext context, RedisService redisService) : ControllerBase
{
    private readonly IDatabase _redis = redisService.GetDatabase();

    [HttpGet]
    [Route("listUsers")]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    public IActionResult ListUsers()
    {
        const string cacheKey = "users";
        var cached = _redis.StringGet(cacheKey);
        if (cached.HasValue)
        {
            var cachedUsers = JsonSerializer.Deserialize<List<User>>(cached.ToString());
            return Ok(cachedUsers);
        }

        var users = context.Users.AsNoTracking().ToList();

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
            var cachedIssuers = JsonSerializer.Deserialize<List<User>>(cached.ToString());
            return Ok(cachedIssuers);
        }

        var tasks = context.Tasks.ToList();
        var taskIssuers = tasks.Select(tk => tk.IssuerId).ToHashSet();
        var issuers = context.Users.Where(u => taskIssuers.Contains(u.Id)).ToList();
        _redis.StringSet(cacheKey, JsonSerializer.Serialize(issuers), TimeSpan.FromMinutes(5));
        return Ok(issuers);
    }
}