using Microsoft.AspNetCore.Mvc;
using Task_Manager.Services;

namespace Task_Manager.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController(RedisService redisService) : ControllerBase
{
    

    [HttpGet]
    [Route("test")]
    public bool Test()
    {
        return redisService.Ping();
    }
    
    [HttpPost]
    [Route("test")]
    public Task Test([FromBody] string test)
    {
        redisService.GetDatabase().StringSet("test", test);
        return Task.CompletedTask;
    }
}