using Microsoft.EntityFrameworkCore;
using Task_Manager.Data;
using Task_Manager.Models;

namespace Task_Manager.Helpers;

public static class CompiledQueries
{
    public static readonly Func<TaskDbContext, string, Task<User?>> GetUserByEmail =
        EF.CompileAsyncQuery((TaskDbContext ctx, string email) => ctx.Users.FirstOrDefault(u => u.Email == email));
}