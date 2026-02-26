namespace Task_Manager.Models.DTO
{
    public record CommentDto(int taskId, int issuerId, string content);
}
