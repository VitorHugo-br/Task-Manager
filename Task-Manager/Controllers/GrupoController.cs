using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task_Manager.Data;
using Task_Manager.Models;
using Task_Manager.Models.DTO;

namespace Task_Manager.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class GrupoController(TaskDbContext context) : ControllerBase
{

    [HttpPost]
    public async Task<ActionResult> CriarGurpo(CriarGrupoDto dto)
    {
        var grupo = new Grupo { Nome = dto.Nome };
        var result = await context.Grupos.AddAsync(grupo);
        await context.SaveChangesAsync();
        return Ok(result.Entity.Id);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GrupoDto>>> ListarGrupos()
    {
        var grupos = await context.Grupos
                                  .AsNoTracking()
                                  .Select(g => new GrupoDto(g.Id, g.Nome, g.Users.Select(u => u.Nome).ToList()))
                                  .ToListAsync();
        return Ok(grupos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Grupo>> ObterGrupo(int id)
    {
        var grupo = await context.Grupos.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
        if (grupo == null) return NotFound();
        return Ok(grupo);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletarGrupo(int id)
    {
        var grupo = await context.Grupos.FindAsync(id);
        if (grupo == null) return NotFound();
        context.Grupos.Remove(grupo);
        await context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult> AtualizarGrupo(int id, string nome)
    {
        var grupo = await context.Grupos.FindAsync(id);
        if (grupo == null) return NotFound();
        grupo.Nome = nome;
        await context.SaveChangesAsync();
        return NoContent();
    }

}
