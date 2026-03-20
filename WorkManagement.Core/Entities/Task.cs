using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkManagement.Core.Enums;
using TaskStatus = WorkManagement.Core.Enums.TaskStatus;

namespace WorkManagement.Core.Entities
{
    public class Task
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TaskPriority Priority { get; set; }
        public TaskStatus Status { get; set; }
        public DateTime? StartDate { get; set; }        // Ngày bắt đầu dự kiến (Manager nhập)
        public DateTime? Deadline { get; set; }
        public int AssignedTo { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? StartedAt { get; set; }        // Set tự động khi Status -> InProgress
        public DateTime? CompletedAt { get; set; }      // Set tự động khi Manager duyệt Completed
        public DateTime? ReviewedAt { get; set; }       // Thời điểm Manager đánh giá (duyệt/từ chối)
        public string? RejectedReason { get; set; }     // Lý do từ chối của Manager
        public DateTime? UpdatedAt { get; set; }        // Cập nhật lần cuối

        // Navigation properties
        public User AssignedUser { get; set; } = null!;
        public User CreatedByUser { get; set; } = null!;
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<FileAttachment> FileAttachments { get; set; } = new List<FileAttachment>();
    }
}
