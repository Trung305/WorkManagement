using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkManagement.Core.Entities;
using WorkManagement.Core.Interfaces.Repositories;
using WorkManagement.Infrastructure.Data;
using Task = System.Threading.Tasks.Task;
using TaskItem = WorkManagement.Core.Entities.Task;
namespace WorkManagement.Infrastructure.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly ApplicationDbContext _db;
        public TaskRepository(ApplicationDbContext db) => _db = db;

        public async Task<TaskItem?> GetByIdAsync(int id)
            => await _db.Tasks.FindAsync(id);

        public async Task<TaskItem?> GetByIdWithUsersAsync(int id)
            => await _db.Tasks
                .Include(t => t.AssignedUser)
                .Include(t => t.CreatedByUser)
                .FirstOrDefaultAsync(t => t.Id == id);

        public async Task<(IEnumerable<TaskItem>, int, Dictionary<int, int>)> GetPagedAsync(
            int page, int pageSize,
            string? search, int? status, int? priority,
            int? assignedToId, int? viewerUserId, int? viewerRole, DateTime? deadlineDate)
        {
            var query = _db.Tasks
                .Include(t => t.AssignedUser)
                .Include(t => t.CreatedByUser)
                .AsQueryable();

            // Phân quyền: User chỉ thấy task của mình
            if (viewerRole == 3 && viewerUserId.HasValue)
                query = query.Where(t => t.AssignedTo == viewerUserId.Value);     
            else if (viewerRole == 2 && viewerUserId.HasValue)
                query = query.Where(t => t.CreatedBy == viewerUserId.Value);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(t =>
                    t.Title.Contains(search) ||
                    (t.Description != null && t.Description.Contains(search)));

            if (status.HasValue)
                query = query.Where(t => (int)t.Status == status.Value);

            if (priority.HasValue)
                query = query.Where(t => (int)t.Priority == priority.Value);

            if (assignedToId.HasValue)
                query = query.Where(t => t.AssignedTo == assignedToId.Value);
            if (deadlineDate.HasValue)
                query = query.Where(t => t.Deadline.HasValue &&
                                         t.Deadline.Value.Date == deadlineDate.Value.Date);
            // Stats (trên toàn bộ filter, trước phân trang)
            var statsQuery = query;
            var stats = await statsQuery
                .GroupBy(t => (int)t.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total, stats);
        }

        public async Task AddAsync(TaskItem task)
        {
            _db.Tasks.Add(task);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(TaskItem task)
        {
            _db.Tasks.Update(task);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(TaskItem task)
        {
            _db.Tasks.Remove(task);
            await _db.SaveChangesAsync();
        }
        public async Task<List<FileAttachment>> GetFilesByTaskIdAsync(int taskId)
    => await _db.FileAttachments.Where(f => f.TaskId == taskId).ToListAsync();

        public async Task<FileAttachment?> GetFileByIdAsync(int fileId)
            => await _db.FileAttachments.FindAsync(fileId);

        public async Task AddFileAsync(FileAttachment file)
        {
            _db.FileAttachments.Add(file);
            await _db.SaveChangesAsync();
        }
        public async Task<List<TaskItem>> GetAllForDashboardAsync(int userId, int userRole)
        {
            var query = _db.Tasks
                .Include(t => t.AssignedUser)
                .Include(t => t.CreatedByUser)
                .AsQueryable();

            query = userRole switch
            {
                1 => query,                                    // Admin: tất cả
                2 => query.Where(t => t.CreatedBy == userId),  // Manager: task mình tạo
                _ => query.Where(t => t.AssignedTo == userId)  // User: task được giao
            };

            return await query.ToListAsync();
        }
        public async Task<List<TaskItem>> GetTasksNeedingReminderAsync()
        {
            var now = DateTime.UtcNow;
            var in12h = now.AddHours(12);
            var in90m = now.AddMinutes(90);

            return await _db.Tasks
                .Where(t => t.Deadline.HasValue
                         && t.Status != WorkManagement.Core.Enums.TaskStatus.Completed
                         && t.Status != WorkManagement.Core.Enums.TaskStatus.Rejected
                         && t.Deadline.Value >= now
                         && t.Deadline.Value <= in12h)
                .ToListAsync();
        }
    }
}
