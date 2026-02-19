namespace Task_Manager.Models.DTO
{
    public record UserDTO(
        string Name,
        string Email,
        string Password,
        string Role
    );

}
