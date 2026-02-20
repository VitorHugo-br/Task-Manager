using Task_Manager.Models.Enums;

namespace Task_Manager.Models.DTO
{
    public class TaskDTO
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public Status Status { get; set; } = Status.Pending;
        public DateTime? StartDate { get; set; } = null;
        public DateTime? EndDate { get; set; } = null;
        public DateTime? DueDate { get; set; } = null;
        public int? UserId { get; set; }
    }
}
