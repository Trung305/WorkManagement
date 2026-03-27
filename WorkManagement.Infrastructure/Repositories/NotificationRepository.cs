using AuthSystem.Application.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkManagement.Core.Entities;
using WorkManagement.Core.Enums;
using WorkManagement.Core.Interfaces.Repositories;
using WorkManagement.Infrastructure.Data;
using Task = System.Threading.Tasks.Task;

namespace WorkManagement.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly ApplicationDbContext _db;

        public NotificationRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Result> AddAsync(Notification notification)
        {
            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result<List<Notification>>> GetByUserIdAsync(int userId)
        {
            var list = await _db.Notifications
                .Where(n => n.UserId == userId && n.Channel == NotificationChannel.InApp)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return Result<List<Notification>>.Success(list);
        }

        public async Task<Result> MarkAsReadAsync(int notificationId)
        {
            var notif = await _db.Notifications.FindAsync(notificationId);
            if (notif == null)
                return Result.Fail("Không tìm thấy thông báo.");

            notif.IsRead = true;
            await _db.SaveChangesAsync();
            return Result.Success();
        }
        public async Task<Notification?> GetByIdAsync(int id)
    => await _db.Notifications.FindAsync(id);

        public async Task UpdateAsync(Notification notification)
        {
            _db.Notifications.Update(notification);
            await _db.SaveChangesAsync();
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            await _db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
        }
        public async Task<List<Notification>> GetUnsentAsync()
        {
            return await _db.Notifications
                .Include(n => n.User)
                .Where(n => !n.IsSent
                         && n.Channel == NotificationChannel.Email
                         && n.FailCount < 3)
                .ToListAsync();
        }
        public async Task<bool> HasReminderAsync(int taskId, int userId, int reminderType)
    => await _db.Notifications.AnyAsync(n =>
        n.TaskId == taskId &&
        n.UserId == userId &&
        n.Type == NotificationType.DeadlineReminder &&
        n.ReminderType == reminderType);
        public async Task MarkAsReadByTaskAsync(int taskId, int userId)
        {
            var notifications = await _db.Notifications
                .Where(n => n.TaskId == taskId && n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var n in notifications)
                n.IsRead = true;

            await _db.SaveChangesAsync();
        }
        public async Task DeleteByTaskAndUserAsync(int taskId, int userId)
        {
            var notifications = await _db.Notifications
                .Where(n => n.TaskId == taskId && n.UserId == userId)
                .ToListAsync();

            _db.Notifications.RemoveRange(notifications);
            await _db.SaveChangesAsync();
        }
        public async Task<List<Notification>> GetRecentAsync(int userId, int count)
        {
            return await _db.Notifications
                .Where(n => n.UserId == userId && n.Channel == NotificationChannel.InApp)
                .OrderByDescending(n => n.CreatedAt)
                .Take(count)
                .ToListAsync();
        }
    }
}