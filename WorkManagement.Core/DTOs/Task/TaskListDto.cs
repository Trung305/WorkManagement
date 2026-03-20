using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkManagement.Core.DTOs.Task
{
    public class TaskListDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public int Priority { get; set; }
        public int Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? Deadline { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? RejectedReason { get; set; }

        // Assigned user
        public int AssignedToId { get; set; }
        public string? AssignedToName { get; set; }
        public string? AssignedToAvatar { get; set; }

        // Creator
        public int CreatedById { get; set; }
        public string CreatedByName { get; set; } = "";
    }

    public class TaskPagedResultDto
    {
        public List<TaskListDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        // Thống kê nhanh cho header
        public int CountPending { get; set; }
        public int CountInProgress { get; set; }
        public int CountPendingReview { get; set; }
        public int CountCompleted { get; set; }
        public int CountRejected { get; set; }
    }
}
