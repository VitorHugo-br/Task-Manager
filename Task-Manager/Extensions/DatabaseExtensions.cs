using Microsoft.EntityFrameworkCore;
using Task_Manager.Data;

namespace Task_Manager.Extensions;

public static class DatabaseExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
        await db.Database.MigrateAsync();
    }
}