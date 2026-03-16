using Task_Manager.Services;

namespace Task_Manager.Models.DTO
{
    public record UserDto(
        string Name,
        string Email,
        string Password,
        string Role = "User"
    )
    {
        public static implicit operator User(UserDto dto) => new User()
        {
            Name = dto.Name,
            Email = dto.Email,
            Password = AuthService.GetHashedPassword(dto.Password),
            Role = dto.Role
        };
    };

}
