using Task_Manager.Models.Enums;

namespace Task_Manager.Models.DTO
{
    public record FilterTasksDto(
        int? TaskId,
        int? UserId,
        int? IssuerId,
        DateTime? CreationDate,
        DateTime? DueDate,
        Status? Status
    );
}
