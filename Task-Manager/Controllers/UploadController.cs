using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Task_Manager.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class UploadController : ControllerBase
{
    [HttpPost("{chamadoId}")]
    public async Task<IActionResult> ProcessarArquivos(int chamadoId, [FromForm] IFormFileCollection arquivos)
    {
        if (arquivos == null || arquivos.Count == 0) return BadRequest();

        var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
        Directory.CreateDirectory(uploadPath);

        foreach (var arquivo in arquivos)
        {
            var filePath = Path.Combine(uploadPath, arquivo.FileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await arquivo.CopyToAsync(stream);
        }

        return Ok();
    }
}
