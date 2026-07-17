using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using Task_Manager.Data;
using Task_Manager.Models.DTO;

namespace Task_Manager.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class GrupoUsuarioController(TaskDbContext context) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateGrupoUsuario(int usuarioId, int grupoId)
    {
        var usuario = await context.Usuarios.FindAsync(usuarioId);
        var grupo = await context.Grupos.FindAsync(grupoId);
        if (usuario == null || grupo == null) return NotFound();
        grupo.Users.Add(usuario);
        await context.SaveChangesAsync();
        return Ok();
    }


    [HttpPost]
    [Route("bulk")]
    public async Task<IActionResult> AdicionarUsuariosAsync(AdicionarUsuariosGrupoDto dto)
    {
        var usuarios = await context.Usuarios.Where(u => dto.UsuariosId.Contains(u.Id)).ToListAsync();
        var grupo = await context.Grupos.FindAsync(dto.GrupoId);
        if(grupo == null) return NotFound();
        grupo.Users.AddRange(usuarios);
        await context.SaveChangesAsync();
        return Ok();
    }
}
