using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;
using System.Security.Claims;
using System.Text.Json;
using Task_Manager.Data;
using Task_Manager.Hubs;
using Task_Manager.Models;
using Task_Manager.Models.DTO;
using Task_Manager.Services;

namespace Task_Manager.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class ChamadoController(TaskDbContext context, RedisService redisService, IHubContext<NotificationHub> wsHub) : ControllerBase
{
    private readonly IDatabase _redis = redisService.GetDatabase();

    [HttpGet]
    public async Task<ActionResult<PagedResponse<Chamado>>> ObterChamados([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1)
            return BadRequest("Page e PageSize devem ser maiores que zero.");

        if (pageSize > 50)
            return BadRequest("PageSize máximo é 50.");

        var cacheKey = $"tasks:page={page}:size={pageSize}";

        var cached = _redis.JSON().Get(cacheKey);
        if (!cached.IsNull)
        {
            var cachedResponse = JsonSerializer.Deserialize<PagedResponse<Chamado>>(cached.ToString());
            return Ok(cachedResponse);
        }

        var query = context.Chamados.AsNoTracking();

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var response = new PagedResponse<Chamado>(
            Items: items,
            Page: page,
            PageSize: pageSize,
            TotalItems: totalItems,
            TotalPages: totalPages
        );

        // 3. Cacheia a página específica
        _redis.JSON().Set(cacheKey, "$", response);
        _redis.KeyExpire(cacheKey, TimeSpan.FromMinutes(10));

        return Ok(response);
    }

    [HttpGet]
    [Route("GetAllToTaskPage")]
    public async Task<Object?> GetAllToTaskPage()
    {
        var everything = context.Chamados
                                .Include(t => t.Responsavel)
                                .Include(t2 => t2.Remetente)
                                .AsNoTracking();
        return Ok(everything);
    }

    [HttpPost]
    public async Task<ActionResult> CriarChamado([FromBody] TaskDto task)
    {
        var userEmail = HttpContext.User.FindFirstValue(ClaimTypes.Email);

        if (string.IsNullOrEmpty(userEmail)) return Unauthorized("User not authenticated");

        var user = await context.Usuarios.FirstOrDefaultAsync(u => u.Email == userEmail);

        if (user == null) return NotFound("User not found");

        Chamado newTask = task;
        newTask.Guid = Guid.NewGuid();
        newTask.RemetenteId = user.Id;

        var chamado = await context.Chamados.AddAsync(newTask);
        await context.SaveChangesAsync();

        if (task.UserId != null)
        {
            await wsHub.Clients
                       .User(chamado.Entity.ResponsavelId.ToString()!)
                       .SendAsync("NovaTarefaNotificacao", new
                       {
                           chamado.Entity.Id,
                           chamado.Entity.Titulo,
                           chamado.Entity.CriadoEm
                       });
        }

        return StatusCode(StatusCodes.Status201Created, chamado.Entity.Id);
    }

    [HttpPatch]
    [Route("UpdateTask/{id}")]
    public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskDto task)
    {
        if (id == 0) return BadRequest("Id must be valid");

        var existingTask = await context.Chamados.FindAsync(id);
        if (existingTask == null) return NotFound("Chamado não encontrado");

        existingTask.Titulo = task.Title;
        existingTask.Descricao = task.Description;
        if (task.Status != existingTask.Status) existingTask.Status = task.Status;
        if (task.StartDate is not null) existingTask.DataInicio = task.StartDate;
        if (task.EndDate is not null) existingTask.DataTermino = task.EndDate;
        if (task.DueDate is not null) existingTask.Prazo = task.DueDate;
        if (task.UserId is not null) existingTask.ResponsavelId = task.UserId;

        context.Entry(existingTask).State = EntityState.Modified;
        await context.SaveChangesAsync();

        _redis.KeyDelete("tasks");
        redisService.RemoveByPattern("tasks-filtered:*");

        return Created();
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<ActionResult<Chamado>> GetChamadoPorIdAsync(int id)
    {
        var chamado = context.Chamados
                             .Include(t => t.Responsavel)
                             .Include(t2 => t2.Remetente)
                             .AsNoTracking()
                             .FirstOrDefault(t => t.Id == id);
        if (chamado == null) return NotFound("Chamado não encontrado");
        return Ok(chamado);
    }

    [HttpPatch]
    [Route("timer")]
    public async Task<ActionResult> AtualizarTempoGasto([FromBody] AtualizarTempoGastoDto dto)
    {
        var chamado = await context.Chamados.FindAsync(dto.ChamadoId);
        if (chamado == null) return NotFound("Chamado não encontrado!");
        if (chamado.HorasGastas < dto.Tempo) chamado.HorasGastas = dto.Tempo;
        if (chamado.HorasGastas > dto.Tempo) chamado.HorasGastas.Add(dto.Tempo);
        await context.SaveChangesAsync();
        return Ok($"Tempo do chamado numero: {chamado.Id} atualizado");
    }

    [HttpDelete]
    public async Task<ActionResult> DeletarChamado(int chamadoId)
    {
        var chamado = await context.Chamados.FindAsync(chamadoId);
        if (chamado == null) return NotFound();
        context.Chamados.Remove(chamado);
        await context.SaveChangesAsync();
        _redis.KeyDelete("tasks");
        return Ok();
    }
}