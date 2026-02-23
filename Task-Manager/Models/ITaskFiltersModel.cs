namespace Task_Manager.Models
{
    public interface ITaskFiltersModel
    {
        DateTime? CreationDate { get; init; }
        DateTime? DueDate { get; init; }
        int? IssuerId { get; init; }
        int? TaskId { get; init; }
        int? UserId { get; init; }
    }
}