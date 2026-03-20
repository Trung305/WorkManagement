using System.ComponentModel.DataAnnotations;
using WorkManagement.Core.DTOs.Task;
using WorkManagement.Core.DTOs.User;

namespace WorkManagement.Web.Models.Task
{
    public class TaskIndexViewModel
    {
        public TaskPagedResultDto PagedResult { get; set; } = new();
        public string? SearchQuery { get; set; }
        public int? StatusFilter { get; set; }
        public int? PriorityFilter { get; set; }
        public int? AssignedToFilter { get; set; }
        public string ViewMode { get; set; } = "table"; // "table" | "kanban"

        // Cho dropdown filter
        public List<UserListDto> Users { get; set; } = new();

        // Role của người đang xem (để render đúng actions)
        public int ViewerRole { get; set; }
    }

    public class TaskFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [MaxLength(200)]
        public string Title { get; set; } = "";

        public string? Description { get; set; }

        [Range(0, 2)]
        public int Priority { get; set; } = 1;
        public List<IFormFile>? Attachments { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? Deadline { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn người thực hiện")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn người thực hiện")]
        public int AssignedToId { get; set; }
        public int Status { get; set; }
        public string? AssignedToName { get; set; }
        public string? RejectedReason { get; set; }

        public List<UserListDto> AssignableUsers { get; set; } = new();

    }

    public class ReviewTaskViewModel
    {
        public int TaskId { get; set; }
        public string TaskTitle { get; set; } = "";
        public bool Approved { get; set; }

        [MaxLength(500)]
        public string? RejectedReason { get; set; }
    }
}
