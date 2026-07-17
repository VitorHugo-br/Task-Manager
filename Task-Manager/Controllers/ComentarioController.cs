using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Task_Manager.Data;
using Task_Manager.Models;
using Task_Manager.Models.DTO;
using Task_Manager.Services;

namespace Task_Manager.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class ComentarioController(TaskDbContext context, RedisService redisService) : ControllerBase
{
    private readonly IDatabase _redis = redisService.GetDatabase();

    [HttpPost]
    [Route("AddComment")]
    public async Task<ActionResult> AddComment([FromBody] CommentDto cmt)
    {
        if (string.IsNullOrEmpty(cmt.Content)) return BadRequest("Content must not be empty");
        if (cmt.TaskId == 0) return BadRequest("Task id must be valid");
        
        await context.Comentarios.AddAsync(cmt);
        await context.SaveChangesAsync();
        
        return Ok("Comment added successfully.");
    }

    [HttpPatch]
    [Route("DeleteComment/")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteComment([FromQuery] int commentId)
    {
        var comment = await context.Comentarios.FirstOrDefaultAsync(c => c.ComentarioId == commentId);
        if (comment == null)
        {
            return NotFound("Comment not found.");
        }

        comment.Deletado = true;
        await context.SaveChangesAsync();
        return Ok("Comment deleted successfully.");
    }

    [HttpPatch]
    [Route("EditComment/")]
    public async Task<ActionResult> EditComment([FromQuery] int commentId, [FromBody] CommentRequestDto updatedComment)
    {
        var comment = await context.Comentarios.FindAsync(commentId);
        if (comment == null)
        {
            return NotFound("Comment not found.");
        }

        comment.Conteudo = updatedComment.Content;
        await context.SaveChangesAsync();
        return Ok("Comment edited successfully.");
    }

    [HttpGet]
    [Route("GetCommentsByTask/{taskId}")]
    public async Task<ActionResult<List<Comentario>>> GetCommentsByTask(int taskId)
    {
        if (taskId == 0) return BadRequest("Task id must be valid");

        var cacheKey = $"comments:task:{taskId}";
        var cached = await _redis.StringGetAsync(cacheKey);
        if (cached.HasValue)
        {
            var cachedComments = JsonSerializer.Deserialize<List<Comentario>>(cached.ToString());
            return Ok(cachedComments);
        }

        var comments = await context.Comentarios
            .Where(c => c.ChamadoId == taskId && !c.Deletado)
            .AsNoTracking()
            .ToListAsync();

        var serialized = JsonSerializer.Serialize(comments);
        await _redis.StringSetAsync(cacheKey, serialized, TimeSpan.FromMinutes(5));

        return Ok(comments);
    }
}