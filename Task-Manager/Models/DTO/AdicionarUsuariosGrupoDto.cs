namespace Task_Manager.Models.DTO
{
    public record AdicionarUsuariosGrupoDto(int GrupoId, IEnumerable<int> UsuariosId);
}