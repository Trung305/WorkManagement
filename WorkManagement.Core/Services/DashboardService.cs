using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkManagement.Core.DTOs.Dashboard;
using WorkManagement.Core.Interfaces.Repositories;
using WorkManagement.Core.Interfaces.Services;
using TaskItem = WorkManagement.Core.Entities.Task;
namespace WorkManagement.Core.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ITaskRepository _taskRepo;

        public DashboardService(ITaskRepository taskRepo)
        {
            _taskRepo = taskRepo;
        }

        public async Task<DashboardStatsDto> GetStatsAsync(int userId, int userRole)
        {
            var now = DateTime.Now;
            var today = now.Date;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var lastWeekStart = weekStart.AddDays(-7);

            // Lấy tasks theo role
            var allTasks = await _taskRepo.GetAllForDashboardAsync(userId, userRole);

            var completed = allTasks.Where(t => t.Status == WorkManagement.Core.Enums.TaskStatus.Completed).ToList();
            var notDone = allTasks.Where(t => t.Status != WorkManagement.Core.Enums.TaskStatus.Completed && t.Status != WorkManagement.Core.Enums.TaskStatus.Rejected).ToList();
            var overdue = notDone.Where(t => t.Deadline.HasValue && t.Deadline.Value < now).ToList();
            var todayTasks = notDone.Where(t => t.Deadline.HasValue && t.Deadline.Value.Date == today).ToList();
            var thisWeekDone = completed.Where(t => t.CompletedAt.HasValue && t.CompletedAt.Value >= weekStart).ToList();
            var lastWeekDone = completed.Where(t => t.CompletedAt.HasValue && t.CompletedAt.Value >= lastWeekStart && t.CompletedAt.Value < weekStart).ToList();

            var total = allTasks.Count;
            var rate = total > 0 ? Math.Round((double)completed.Count / total * 100, 1) : 0;

            // Upcoming — task chưa xong, sắp deadline
            var upcoming = notDone
                .OrderBy(t => t.Deadline ?? DateTime.MaxValue)
                .Take(5)
                .Select(MapToDto)
                .ToList();

            // Near deadline — deadline trong 3 ngày tới
            var nearDeadline = notDone
                .Where(t => t.Deadline.HasValue && t.Deadline.Value.Date <= today.AddDays(3) && t.Deadline.Value >= now)
                .OrderBy(t => t.Deadline)
                .Take(5)
                .Select(MapToDto)
                .ToList();

            // Calendar deadline days trong tháng hiện tại
            var deadlineDays = allTasks
                .Where(t => t.Deadline.HasValue
                         && t.Deadline.Value.Month == now.Month
                         && t.Deadline.Value.Year == now.Year)
                .Select(t => t.Deadline!.Value.Day)
                .Distinct()
                .ToList();

            return new DashboardStatsDto
            {
                TotalTasks = total,
                TodayTasks = todayTasks.Count,
                OverdueTasks = overdue.Count,
                CompletedThisWeek = thisWeekDone.Count,
                CompletedLastWeek = lastWeekDone.Count,
                CompletionRate = rate,
                UpcomingTasks = upcoming,
                NearDeadlineTasks = nearDeadline,
                DeadlineDays = deadlineDays,
                Today = now.Day,
                CurrentMonth = now.Month,
                CurrentYear = now.Year,
            };
        }
        public async Task<StatsDto> GetDetailedStatsAsync(int userId, int userRole, DateTime? from = null, DateTime? to = null)
        {
            var now = DateTime.Now;
            var tasks = await _taskRepo.GetAllForDashboardAsync(userId, userRole);

            // Filter 
            if (from.HasValue)
                tasks = tasks.Where(t => t.CreatedAt >= from.Value).ToList();
            if (to.HasValue)
                tasks = tasks.Where(t => t.CreatedAt <= to.Value.AddDays(1)).ToList();

            // Tổng quan
            var dto = new StatsDto
            {
                TotalTasks = tasks.Count,
                Pending = tasks.Count(t => t.Status == WorkManagement.Core.Enums.TaskStatus.Pending),
                InProgress = tasks.Count(t => t.Status == WorkManagement.Core.Enums.TaskStatus.InProgress),
                PendingReview = tasks.Count(t => t.Status == WorkManagement.Core.Enums.TaskStatus.PendingReview),
                Completed = tasks.Count(t => t.Status == WorkManagement.Core.Enums.TaskStatus.Completed),
                Rejected = tasks.Count(t => t.Status == WorkManagement.Core.Enums.TaskStatus.Rejected),
            };

            // Đúng hạn vs quá hạn
            var completedTasks = tasks.Where(t => t.Status == WorkManagement.Core.Enums.TaskStatus.Completed).ToList();
            dto.OnTime = completedTasks.Count(t => t.Deadline.HasValue && t.CompletedAt.HasValue && t.CompletedAt <= t.Deadline);
            dto.Overdue = completedTasks.Count(t => t.Deadline.HasValue && t.CompletedAt.HasValue && t.CompletedAt > t.Deadline)
                        + tasks.Count(t => t.Status != WorkManagement.Core.Enums.TaskStatus.Completed && t.Status != WorkManagement.Core.Enums.TaskStatus.Rejected
                                        && t.Deadline.HasValue && t.Deadline < now);

            // Hoàn thành theo 7 ngày gần nhất
            var weekly = new List<WeeklyStatsDto>();
            for (int i = 6; i >= 0; i--)
            {
                var day = now.Date.AddDays(-i);
                var dayTasks = tasks.Where(t => t.Deadline.HasValue && t.Deadline.Value.Date == day).ToList();
                weekly.Add(new WeeklyStatsDto
                {
                    Label = day.ToString("ddd", new System.Globalization.CultureInfo("vi-VN")),
                    Completed = dayTasks.Count(t => t.Status == WorkManagement.Core.Enums.TaskStatus.Completed),
                    Assigned = dayTasks.Count
                });
            }
            dto.WeeklyCompleted = weekly;

            // Hiệu suất thành viên (chỉ Admin/Manager)
            if (userRole <= 2)
            {
                dto.MemberStats = tasks
                    .GroupBy(t => new { t.AssignedTo, Name = t.AssignedUser?.FullName ?? "—" })
                    .Select(g => new MemberStatsDto
                    {
                        UserId = g.Key.AssignedTo,
                        FullName = g.Key.Name,
                        Total = g.Count(),
                        Completed = g.Count(t => t.Status == WorkManagement.Core.Enums.TaskStatus.Completed),
                        InProgress = g.Count(t => t.Status == WorkManagement.Core.Enums.TaskStatus.InProgress),
                        Overdue = g.Count(t => t.Status != WorkManagement.Core.Enums.TaskStatus.Completed
                                               && t.Status != WorkManagement.Core.Enums.TaskStatus.Rejected
                                               && t.Deadline.HasValue && t.Deadline < now)
                    })
                    .OrderByDescending(m => m.CompletionRate)
                    .ToList();
            }

            return dto;
        }
        private static DashboardTaskDto MapToDto(TaskItem t) => new()
        {
            Id = t.Id,
            Title = t.Title,
            Status = (int)t.Status,
            Priority = (int)t.Priority,
            Deadline = t.Deadline,
            AssignedToName = t.AssignedUser?.FullName,
            CreatedByName = t.CreatedByUser?.FullName,
        };
    }
}
