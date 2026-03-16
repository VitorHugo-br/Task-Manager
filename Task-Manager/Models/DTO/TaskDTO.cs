using Task_Manager.Models.Enums;

namespace Task_Manager.Models.DTO
{
    public class TaskDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Status Status { get; set; } = Status.Pending;
        public DateTime? StartDate { get; set; } = null;
        public DateTime? EndDate { get; set; } = null;
        public DateTime? DueDate { get; set; } = null;
        public int? UserId { get; set; }
        
        public static implicit operator MyTask(TaskDto taskDto) => new MyTask
        {
            Title = taskDto.Title,
            Description = taskDto.Description,
            Status = taskDto.Status,
            StartDate = taskDto.StartDate,
            EndDate = taskDto.EndDate,
            DueDate = taskDto.DueDate,
            UserId = taskDto.UserId
        };
    }
}
