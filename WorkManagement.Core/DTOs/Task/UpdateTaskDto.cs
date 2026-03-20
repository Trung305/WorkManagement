using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkManagement.Core.DTOs.Task
{
    public class UpdateTaskDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public int Priority { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? Deadline { get; set; }
        public int AssignedToId { get; set; }
    }

    public class UpdateTaskStatusDto
    {
        public int Id { get; set; }
        public int NewStatus { get; set; }
    }

    public class ReviewTaskDto
    {
        public int Id { get; set; }
        public bool Approved { get; set; }
        public string? RejectedReason { get; set; }
        public int ReviewedById { get; set; }
    }
}
