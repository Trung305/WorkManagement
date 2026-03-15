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

        // Login bằng email/password
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }

        // Login bằng Google
        public string? GoogleId { get; set; }

        // Thông tin user
        public string FullName { get; set; }
        public string? AvatarUrl { get; set; }

        // Role hệ thống
        public string Role { get; set; }   // Admin / Manager / User

        // Trạng thái
        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        // Navigation
        public ICollection<Task> CreatedTasks { get; set; }
        public ICollection<Task> AssignedTasks { get; set; }
        public ICollection<Notification> Notifications { get; set; }
    }
}
