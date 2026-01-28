using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkManagement.Core.Entities
{
    public class Task
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int Priority { get; set; }
        public int Status { get; set; }  
        public DateTime Deadline { get; set; }
        public int AssignedTo { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public User AssignedToUser { get; set; }
        public User CreatedByUser { get; set; }
        public ICollection<Notification> Notifications { get; set; }
    }
}
