using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WorkManagement.Core.Enums;
using WorkManagement.Core.Interfaces.Repositories;
using WorkManagement.Core.Interfaces.Services;

namespace WorkManagement.Infrastructure.BackgroundJobs
{
    public class DeadlineReminderJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DeadlineReminderJob> _logger;

        public DeadlineReminderJob(IServiceScopeFactory scopeFactory, ILogger<DeadlineReminderJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("DeadlineReminderJob started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckDeadlinesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in DeadlineReminderJob");
                }

                // Chạy mỗi 1 giờ
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task CheckDeadlinesAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var taskRepo = scope.ServiceProvider.GetRequiredService<ITaskRepository>();
            var notifService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var notifRepo = scope.ServiceProvider.GetRequiredService<INotificationRepository>(); // ← thêm

            var now = DateTime.Now; // ← đổi UtcNow → Now
            var tasks = await taskRepo.GetTasksNeedingReminderAsync();

            foreach (var task in tasks)
            {
                if (!task.Deadline.HasValue) continue;
                var timeLeft = task.Deadline.Value - now;

                // Check 1.5h TRƯỚC
                if (timeLeft.TotalMinutes <= 90 && timeLeft.TotalMinutes > 80)
                {
                    var alreadySent = await notifRepo.HasReminderAsync(task.Id, task.AssignedTo, 2);
                    if (!alreadySent)
                    {
                        await notifService.AddAsync(
                            userId: task.AssignedTo,
                            taskId: task.Id,
                            type: NotificationType.DeadlineReminder,
                            title: $"Công việc \"{task.Title}\" còn 1 tiếng 30 phút nữa đến hạn!",
                            channel: NotificationChannel.InApp,
                            reminderType: 2
                        );
                        await notifService.AddAsync(
                            userId: task.AssignedTo,
                            taskId: task.Id,
                            type: NotificationType.DeadlineReminder,
                            title: $"Công việc \"{task.Title}\" còn 1 tiếng 30 phút nữa đến hạn!",
                            channel: NotificationChannel.Email,
                            reminderType: 2
                        );
                    }
                }
                else if (timeLeft.TotalHours <= 12 && timeLeft.TotalHours > 11)
                {
                    var alreadySent = await notifRepo.HasReminderAsync(task.Id, task.AssignedTo, 1);
                    if (!alreadySent)
                    {
                        await notifService.AddAsync(
                            userId: task.AssignedTo,
                            taskId: task.Id,
                            type: NotificationType.DeadlineReminder,
                            title: $"Công việc \"{task.Title}\" còn 12 giờ nữa đến hạn!",
                            channel: NotificationChannel.InApp,
                            reminderType: 1
                        );
                        await notifService.AddAsync(
                           userId: task.AssignedTo,
                           taskId: task.Id,
                           type: NotificationType.DeadlineReminder,
                           title: $"Công việc \"{task.Title}\" còn 12 giờ nữa đến hạn!",
                           channel: NotificationChannel.Email,
                           reminderType: 2
                       );
                    }
                }
            }
        }
    }
}