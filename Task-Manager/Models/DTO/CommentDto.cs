namespace Task_Manager.Models.DTO
{
    public record CommentDto(int TaskId, int IssuerId, string Content)
    {
        public static implicit operator Comentario(CommentDto comm) => new Comentario()
        {
            ChamadoId = comm.TaskId,
            RemetenteId = comm.IssuerId,
            Conteudo = comm.Content
        };
    };
}
