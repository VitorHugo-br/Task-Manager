
namespace Task_Manager.Models
{
    public record TaskFiltersModel(int? Id, int? UserId, int? IssuerId, DateTime? CreationDate, DateTime? DueDate);
}
