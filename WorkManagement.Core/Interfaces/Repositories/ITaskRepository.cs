using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkManagement.Core.Entities;
using Task = System.Threading.Tasks.Task;
using TaskItem = WorkManagement.Core.Entities.Task;
namespace WorkManagement.Core.Interfaces.Repositories
{
    public interface ITaskRepository
    {
        Task<TaskItem?> GetByIdAsync(int id);
        Task<TaskItem?> GetByIdWithUsersAsync(int id);
        Task<(IEnumerable<TaskItem> Items, int Total, Dictionary<int, int> Stats)> GetPagedAsync(
            int page, int pageSize,
            string? search, int? status, int? priority,
            int? assignedToId, int? viewerUserId, int? viewerRole, DateTime? deadlineDate);
        Task AddAsync(TaskItem task);
        Task UpdateAsync(TaskItem task);
        Task DeleteAsync(TaskItem task);
        Task<List<FileAttachment>> GetFilesByTaskIdAsync(int taskId);
        Task<FileAttachment?> GetFileByIdAsync(int fileId);
        Task AddFileAsync(FileAttachment file);
        Task DeleteFileAsync(FileAttachment file);
        Task<List<TaskItem>> GetAllForDashboardAsync(int userId, int userRole);

        Task<List<TaskItem>> GetTasksNeedingReminderAsync();
    }
}
