using System.ComponentModel.DataAnnotations;
using Task_Manager.Models.Enums;

namespace Task_Manager.Models
{
    public class MyTask
    {
        [Key]
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public Status Status { get; set; } = Status.Pending;
        public DateTime? StartDate { get; set; } = null;
        public DateTime? EndDate { get; set; } = null;
        public DateTime? DueDate { get; set; }
        public required string RequestedBy { get; set; }
        public int? UserId { get; set; }
        public User? User { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

    }
}
