using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkManagement.Core.Enums;

namespace WorkManagement.Core.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? TaskId { get; set; }
        public NotificationType Type { get; set; }             // NotificationType enum
        public int? ReminderType { get; set; }  // 1=Nhắc 12h, 2=Nhắc 1h30 — tránh gửi trùng
        public string Title { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public NotificationChannel Channel { get; set; }        // NotificationChannel enum
        public bool IsSent { get; set; } = false;
        public DateTime? SentAt { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public Task? Task { get; set; }
    }
}
