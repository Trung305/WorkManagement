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
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task CheckDeadlinesAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var taskRepo = scope.ServiceProvider.GetRequiredService<ITaskRepository>();
            var notifService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var now = DateTime.UtcNow;
            var tasks = await taskRepo.GetTasksNeedingReminderAsync();

            foreach (var task in tasks)
            {
                if (!task.Deadline.HasValue) continue;

                var timeLeft = task.Deadline.Value - now;

                // Nhắc 12 tiếng trước deadline (ReminderType = 1)
                if (timeLeft.TotalHours <= 12 && timeLeft.TotalHours > 11)
                {
                    await notifService.AddAsync(
                        userId: task.AssignedTo,
                        taskId: task.Id,
                        type: NotificationType.DeadlineReminder,
                        title: $"⏰ Task \"{task.Title}\" còn 12 giờ nữa đến hạn!",
                        channel: NotificationChannel.InApp,
                        reminderType: 1
                    );
                    _logger.LogInformation("Sent 12h reminder for task {Id}", task.Id);
                }

                // Nhắc 1.5 tiếng trước deadline (ReminderType = 2)
                if (timeLeft.TotalMinutes <= 90 && timeLeft.TotalMinutes > 80)
                {
                    await notifService.AddAsync(
                        userId: task.AssignedTo,
                        taskId: task.Id,
                        type: NotificationType.DeadlineReminder,
                        title: $"⏰ Task \"{task.Title}\" còn 1 tiếng 30 phút nữa đến hạn!",
                        channel: NotificationChannel.InApp,
                        reminderType: 2
                    );
                    _logger.LogInformation("Sent 1.5h reminder for task {Id}", task.Id);
                }
            }
        }
    }
}