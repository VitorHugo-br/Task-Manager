using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Task_Manager.Models;

public class AuditLog
{
    [Key]
    [Column("audit_log_id")]
    public Guid AuditLogId { get; set; } = Guid.NewGuid();
    
    [Column("action")]
    [StringLength(50)]
    public string Action { get; set; } = string.Empty;
    
    [Column("entity_name")]
    [StringLength(50)]
    public string EntityName { get; set; } = string.Empty;
    
    [Column("entity_id")]
    public int? EntityId { get; set; }
    
    [Column("old_values")]
    [StringLength(2000)]
    public string? OldValues { get; set; }
    
    [Column("new_values")]
    [StringLength(2000)]
    public string? NewValues { get; set; }
    
    [Column("user_id")]
    public int? UserId { get; set; }
    public Usuario? User { get; set; }
    
    [Column("ip_address")]
    [StringLength(50)]
    public string? IpAddress { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
}   