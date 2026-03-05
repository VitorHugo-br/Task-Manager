namespace Task_Manager.Models.DTO;

public record AuditLogDto(
    string Action,
    string EntityName,
    int? EntityId,
    int? UserId,
    string? OldValues = null,
    string? NewValues = null,
    string? Ip = null
);