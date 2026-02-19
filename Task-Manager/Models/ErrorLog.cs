using System.ComponentModel.DataAnnotations;

namespace Task_Manager.Models
{
    public class ErrorLog
    {
        [Key]
        public int Id { get; set; }
        public string? Error { get; set; }
        public DateTimeOffset Timestamp { get; set; }

    }
}
