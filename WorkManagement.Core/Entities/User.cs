using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkManagement.Core.Enums;

namespace WorkManagement.Core.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PasswordHash { get; set; }
        public UserRole Role { get; set; }           // UserRole enum: 0=Admin, 1=User, 2=Manager
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? AvatarUrl { get; set; }
        public string? GoogleId { get; set; }   // Dự phòng đăng nhập Google OAuth
        public DateTime? LastLoginAt { get; set; }

        public int  Status { get; set; }

        // Navigation properties
        public ICollection<Task> AssignedTasks { get; set; } = new List<Task>();
        public ICollection<Task> CreatedTasks { get; set; } = new List<Task>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<FileAttachment> UploadedFiles { get; set; } = new List<FileAttachment>();
    }
}
