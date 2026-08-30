using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Task_Manager.Services;

namespace Task_Manager.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class UploadController(MinIoStorageService storageService) : ControllerBase
{
    [HttpPost("{chamadoId}")]
    public async Task<IActionResult> ProcessarArquivos(int chamadoId, [FromForm] IFormFileCollection arquivos)
    {
        if (arquivos == null || arquivos.Count == 0) return BadRequest();

        foreach(var arquivo in arquivos)
        {
            var objectName = $"{chamadoId}/{arquivo.FileName}";
            using var stream = arquivo.OpenReadStream();

            await storageService.UploadFileAsync("taskmanager-arquivos", objectName, stream, arquivo.ContentType);
        }

        return Ok();
    }
}
