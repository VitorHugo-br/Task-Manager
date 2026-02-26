namespace Task_Manager.Models.DTO
{
    public record UserDto(
        string Name,
        string Email,
        string Password,
        string Role = "User"
    );

}
