using Microsoft.AspNetCore.Mvc;

namespace Task_Manager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : Controller
    {

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            return Ok();

        }
    }
}
