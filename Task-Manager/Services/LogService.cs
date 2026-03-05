using Task_Manager.Data;
using Task_Manager.Interfaces;
using Task_Manager.Models;

namespace Task_Manager.Services;

public class LogService(TaskDbContext context) : ILogService
{
    public async Task Info(string message, string? source = null)
    {
        var log = new SystemLog
        {
            SystemLogId = Guid.NewGuid(),
            Message = message,
            Level = "INFO",
            Source = source
        };
        await context.SystemLogs.AddAsync(log);
        await context.SaveChangesAsync();
    }

    public async Task Error(string message, Exception ex, string? source = null)
    {
        var log = new SystemLog
        {
            SystemLogId = Guid.NewGuid(),
            Level = "ERROR",
            Message = message,
            Exception = ex.ToString(),
            Source = source
        };

        await context.SystemLogs.AddAsync(log);
        await context.SaveChangesAsync();
    }
}