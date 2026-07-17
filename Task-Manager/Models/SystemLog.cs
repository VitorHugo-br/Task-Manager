using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Task_Manager.Models;

public class SystemLog
{
    [Key]
    [Column("system_log_id")]
    public Guid SystemLogId { get; set; } = Guid.NewGuid();

    [Column("level")]
    public string Level { get; set; } = null!;
    // INFO, WARNING, ERROR, DEBUG

    [Column("message")]
    public string Message { get; set; } = null!;

    [Column("exception")]
    public string? Exception { get; set; }

    [Column("source")]
    public string? Source { get; set; }

    [Column("user_id")]
    public int? UserId { get; set; }
    public Usuario? User { get; set; }
    
    [Column("trace_id")]
    public string? TraceId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}