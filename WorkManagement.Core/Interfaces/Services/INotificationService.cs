using AuthSystem.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkManagement.Core.DTOs.Notification;
using WorkManagement.Core.Enums;

namespace WorkManagement.Core.Interfaces.Services
{
    public interface INotificationService
    {
        Task<Result> AddAsync(int userId, int? taskId, NotificationType type, string title,
                              NotificationChannel channel = NotificationChannel.InApp,
                              int? reminderType = null);
        Task<Result<List<NotificationDto>>> GetByUserIdAsync(int userId);
        Task<Result> MarkAsReadAsync(int notificationId, int userId);
        Task<Result> MarkAllAsReadAsync(int userId);

    }
}
