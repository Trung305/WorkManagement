using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkManagement.Core.Enums;
using WorkManagement.Core.Interfaces.Repositories;
using WorkManagement.Infrastructure.Services;
using WorkManagement.Core.Interfaces.Services;

namespace WorkManagement.Infrastructure.BackgroundJobs
{
    public class NotificationBackgroundJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NotificationBackgroundJob> _logger;

        public NotificationBackgroundJob(IServiceScopeFactory scopeFactory, ILogger<NotificationBackgroundJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("NotificationBackgroundJob started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingNotificationsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in NotificationBackgroundJob");
                }

                // Chạy mỗi 1 phút
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task ProcessPendingNotificationsAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var notifRepo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var pending = await notifRepo.GetUnsentAsync();

            foreach (var notif in pending)
            {
                if (notif.FailCount >= 3)
                {
                    _logger.LogWarning("Notification {Id} skipped — failed {Count} times",
                        notif.Id, notif.FailCount);
                    continue;
                }

                try
                {
                    if (notif.Channel == NotificationChannel.Email)
                    {
                        var toEmail = notif.User?.Email;
                        if (!string.IsNullOrEmpty(toEmail))
                        {
                            await emailService.SendAsync(
                                toEmail: toEmail,
                                subject: notif.Title,
                                body: $@"
                            <div style='font-family:sans-serif;max-width:600px;margin:0 auto;'>
                                <h2 style='color:#4f46e5;'>WorkManagement</h2>
                                <p>{notif.Title}</p>
                                <hr/>
                                <p style='color:#9ca3af;font-size:12px;'>
                                    Đây là email tự động từ hệ thống WorkManagement.
                                </p>
                            </div>"
                            );
                        }
                    }

                    notif.IsSent = true;
                    notif.SentAt = DateTime.Now;
                    await notifRepo.UpdateAsync(notif);
                    _logger.LogInformation("Sent notification {Id}", notif.Id);
                }
                catch (Exception ex)
                {
                    notif.FailCount++;
                    await notifRepo.UpdateAsync(notif);

                    _logger.LogError(ex,
                        "Failed to send notification {Id} (attempt {Count}/3)",
                        notif.Id, notif.FailCount);
                }
            }
        }
    }
}
