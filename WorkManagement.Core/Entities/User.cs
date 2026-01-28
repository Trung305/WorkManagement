using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkManagement.Core.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string? ApplicationUserId { get; set; }  // FK → AspNetUsers.Id
        public string FullName { get; set; }
        public string Email { get; set; }
        public int Role { get; set; }  // Admin / Manager / User
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public ICollection<Task> CreatedTasks { get; set; }
        public ICollection<Task> AssignedTasks { get; set; }
        public ICollection<Notification> Notifications { get; set; }
    }
}
