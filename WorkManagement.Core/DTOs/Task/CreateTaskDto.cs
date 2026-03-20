using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkManagement.Core.DTOs.Task
{
    public class CreateTaskDto
    {
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public int Priority { get; set; } = 1; // Medium
        public DateTime? StartDate { get; set; }
        public DateTime? Deadline { get; set; }
        public int AssignedToId { get; set; }
        public int CreatedById { get; set; }
    }
}
