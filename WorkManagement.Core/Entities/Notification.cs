using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkManagement.Core.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? TaskId { get; set; }
        public int Type { get; set; }  // TaskAssigned / Deadline
        public string Title { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Channel { get; set; }  // InApp / Email / SMS
        public bool IsSent { get; set; }
        public DateTime? SentAt { get; set; }

        // Navigation properties
        public User User { get; set; }
        public Task Task { get; set; }
    }
}
