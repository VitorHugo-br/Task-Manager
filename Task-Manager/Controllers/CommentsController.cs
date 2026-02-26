using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task_Manager.Data;
using Task_Manager.Models;
using Task_Manager.Models.DTO;

namespace Task_Manager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class CommentsController : ControllerBase
    {

        private readonly TaskDbContext _context;

        public CommentsController(TaskDbContext context)
        {
            _context = context;
        }

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

            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
            return Ok("Comment added successfully.");
        }

        //TODO: Limitar a edição e exclusão de comentários apenas para o autor ou para administradores
        [HttpDelete]
        [Route("DeleteComment/{commentId}")]
        [Authorize(Roles = "Admin")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult> DeleteComment(int commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
            {
                return NotFound("Comment not found.");
            }
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return Ok("Comment deleted successfully.");
        }

        [HttpPatch]
        [Route("EditComment/{commentId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> EditComment(int commentId, [FromBody] string newContent)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
            {
                return NotFound("Comment not found.");
            }
            comment.Content = newContent;
            await _context.SaveChangesAsync();
            return Ok("Comment edited successfully.");
        }

        [HttpGet]
        [Route("GetCommentsByTask/{taskId}")]
        public async Task<ActionResult<List<Comment>>> GetCommentsByTask(int taskId)
        {
            var comments = await _context.Comments.Where(c => c.TaskId == taskId).ToListAsync();
            return Ok(comments);

        }
    }
}
