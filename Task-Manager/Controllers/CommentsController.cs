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

[ApiController]
[Route("[controller]")]
[Authorize]
public class CommentsController(TaskDbContext context, RedisService redisService) : ControllerBase
{
    private readonly IDatabase _redis = redisService.GetDatabase();

    [HttpPost]
    [Route("AddComment")]
    public async Task<ActionResult> AddComment([FromBody] CommentDto cmt)
    {
        var comment = new Comment
        {
            TaskId = cmt.taskId,
            IssuerId = cmt.issuerId,
            Content = cmt.content
        };

        await context.Comments.AddAsync(comment);
        await context.SaveChangesAsync();
        return Ok("Comment added successfully.");
    }

    //TODO: Limitar a edição e exclusão de comentários apenas para o autor ou para administradores
    [HttpPatch]
    [Route("DeleteComment/")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteComment([FromQuery] int commentId)
    {
        var comment = await context.Comments.FirstOrDefaultAsync(c => c.CommentId == commentId);
        if (comment == null)
        {
            return NotFound("Comment not found.");
        }

        comment.IsDeleted = true;
        await context.SaveChangesAsync();
        return Ok("Comment deleted successfully.");
    }

    [HttpPatch]
    [Route("EditComment/")]
    public async Task<ActionResult> EditComment([FromQuery] int commentId, [FromBody] CommentRequestDto updatedComment)
    {
        var comment = await context.Comments.FindAsync(commentId);
        if (comment == null)
        {
            return NotFound("Comment not found.");
        }

        comment.Content = updatedComment.Content;
        await context.SaveChangesAsync();
        return Ok("Comment edited successfully.");
    }

    [HttpGet]
    [Route("GetCommentsByTask/{taskId}")]
    public async Task<ActionResult<List<Comment>>> GetCommentsByTask(int taskId)
    {
        if (taskId == 0) return BadRequest("Task id must be valid");

        var cacheKey = $"comments:task:{taskId}";
        var cached = await _redis.StringGetAsync(cacheKey);
        if (cached.HasValue)
        {
            var cachedComments = JsonSerializer.Deserialize<List<Comment>>(cached.ToString());
            return Ok(cachedComments);
        }

        var comments = await context.Comments
            .Where(c => c.TaskId == taskId && !c.IsDeleted)
            .AsNoTracking()
            .ToListAsync();

        var serialized = JsonSerializer.Serialize(comments);
        await _redis.StringSetAsync(cacheKey, serialized, TimeSpan.FromMinutes(5));

        return Ok(comments);
    }
}