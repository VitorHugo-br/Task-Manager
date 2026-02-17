using Task_Manager.Models.Enums;

namespace Task_Manager.Models
{
    public class MyTask
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public Status Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime DueDate { get; set; }
        public required string RequestedBy { get; set; }
        public User? SignedTo { get; set; }

    }
}
