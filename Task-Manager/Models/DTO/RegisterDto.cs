using Task_Manager.Services;

namespace Task_Manager.Models.DTO;

public record RegisterDto(
    string Name,
    string Email,
    string Password
)
{
    public static implicit operator Usuario(RegisterDto dto) => new Usuario()
    {
        Nome = dto.Name,
        Email = dto.Email,
        Senha = AuthService.GetHashedPassword(dto.Password),
    };
};
