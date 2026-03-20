using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkManagement.Core.DTOs.Notification
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? TaskId { get; set; }
        public int Type { get; set; }
        public string Title { get; set; } = "";
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? SenderName { get; set; }
    }
}
