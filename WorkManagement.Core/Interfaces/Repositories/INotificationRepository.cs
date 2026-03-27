using AuthSystem.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkManagement.Core.Entities;

namespace WorkManagement.Core.Interfaces.Repositories
{
    public interface INotificationRepository
    {
        System.Threading.Tasks.Task<Result> AddAsync(Notification notification);
        System.Threading.Tasks.Task<Result<List<Notification>>> GetByUserIdAsync(int userId);
        System.Threading.Tasks.Task<Result> MarkAsReadAsync(int notificationId);
        System.Threading.Tasks.Task<Notification?> GetByIdAsync(int id);
        System.Threading.Tasks.Task UpdateAsync(Notification notification);
        System.Threading.Tasks.Task MarkAllAsReadAsync(int userId);
        System.Threading.Tasks.Task<List<Notification>> GetUnsentAsync();
        System.Threading.Tasks.Task<bool> HasReminderAsync(int taskId, int userId, int reminderType);
        System.Threading.Tasks.Task MarkAsReadByTaskAsync(int taskId, int userId);
        System.Threading.Tasks.Task DeleteByTaskAndUserAsync(int taskId, int userId);
        System.Threading.Tasks.Task<List<Notification>> GetRecentAsync(int userId, int count);
    }
}
