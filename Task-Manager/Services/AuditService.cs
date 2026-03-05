using Task_Manager.Data;
using Task_Manager.Models;
using Task_Manager.Models.DTO;

namespace Task_Manager.Services;

public class AuditService(TaskDbContext context)
{

    public async Task Log(AuditLogDto auditLog)
    {
        var audit = new AuditLog
        {
            AuditLogId = Guid.NewGuid(),
            Action = auditLog.Action,
            EntityName = auditLog.EntityName,
            EntityId = auditLog.EntityId,
            UserId = auditLog.UserId,
            OldValues = auditLog.OldValues,
            NewValues = auditLog.NewValues,
            IpAddress = auditLog.Ip
        };
        await context.AuditLogs.AddAsync(audit);
        await context.SaveChangesAsync();
    }
}