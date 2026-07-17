using Task_Manager.Models;

namespace Task_Manager.Interfaces;

public interface IAuthService
{
    string GenerateToken(Usuario user);

}
