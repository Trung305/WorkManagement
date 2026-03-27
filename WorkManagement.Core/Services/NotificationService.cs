using AuthSystem.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkManagement.Core.DTOs.Notification;
using WorkManagement.Core.Entities;
using WorkManagement.Core.Enums;
using WorkManagement.Core.Interfaces.Repositories;
using WorkManagement.Core.Interfaces.Services;

namespace WorkManagement.Core.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<Result> AddAsync(int userId, int? taskId, NotificationType type, string title,
                                           NotificationChannel channel = NotificationChannel.InApp,
                                           int? reminderType = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                TaskId = taskId,
                Type = type,
                Title = title,
                Channel = channel,
                ReminderType = reminderType,
                IsRead = false,
                IsSent = false,
                CreatedAt = DateTime.Now
            };
            await _notificationRepository.AddAsync(notification);
            return Result.Success();
        }

        public async Task<Result<List<NotificationDto>>> GetByUserIdAsync(int userId)
        {
            var result = await _notificationRepository.GetByUserIdAsync(userId);
            var list = result.Data ?? new();  

            var dtos = list.Select(n => new NotificationDto
            {
                Id = n.Id,
                UserId = n.UserId,
                TaskId = n.TaskId,
                Type = (int)n.Type,
                Title = n.Title,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            }).ToList();
            return Result<List<NotificationDto>>.Success(dtos);
        }
        public async Task<Result> MarkAsReadAsync(int notificationId, int userId)
        {
            var notif = await _notificationRepository.GetByIdAsync(notificationId);
            if (notif == null) return Result.Fail("Không tìm thấy thông báo.");
            if (notif.UserId != userId) return Result.Fail("Không có quyền.");

            notif.IsRead = true;
            await _notificationRepository.UpdateAsync(notif);
            return Result.Success();
        }

        public async Task<Result> MarkAllAsReadAsync(int userId)
        {
            await _notificationRepository.MarkAllAsReadAsync(userId);
            return Result.Success();
        }
        public async System.Threading.Tasks.Task MarkAsReadByTaskAsync(int taskId, int userId)
        {
            await _notificationRepository.MarkAsReadByTaskAsync(taskId, userId);
        }
        public async Task<List<NotificationDto>> GetRecentAsync(int userId, int count)
        {
            var notifications = await _notificationRepository.GetRecentAsync(userId, count);
            return notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                TaskId = n.TaskId
            }).ToList();
        }
    }
}
