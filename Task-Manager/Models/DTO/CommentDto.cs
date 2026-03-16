namespace Task_Manager.Models.DTO
{
    public record CommentDto(int TaskId, int IssuerId, string Content)
    {
        public static implicit operator Comment(CommentDto comm) => new Comment()
        {
            TaskId = comm.TaskId,
            IssuerId = comm.IssuerId,
            Content = comm.Content
        };
    };
}
