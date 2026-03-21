using Task_Manager.Interfaces;
using Task_Manager.Services;

namespace Task_Manager.Extensions;

public static class ServicesExtensions
{
    public static void AddMyServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddTransient<AuthService>();
        builder.Services.AddTransient<RedisService>();
        builder.Services.AddTransient<AuditService>();
        builder.Services.AddScoped<ILogService, LogService>();
    }
}