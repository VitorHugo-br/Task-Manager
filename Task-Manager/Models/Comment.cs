using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Task_Manager.Models
{
    public class Comment
    {
        [Key]
        [Column("comment_id")]
        public int ComentId { get; set; }

        [Column("task_id")]
        public int TaskId { get; set; }
        public MyTask Task { get; set; }

        [Column("issuer_id")]
        public int IssuerId { get; set; }
        public User Issuer { get; set; }

        [Column("content")]
        public string Content { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
