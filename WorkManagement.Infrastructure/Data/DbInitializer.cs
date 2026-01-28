using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkManagement.Core.Entities;
using WorkManagement.Core.Enums;
using StatusTask = WorkManagement.Core.Enums.TaskStatus;
namespace WorkManagement.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Users.Any())
            {
                return; 
            }

            var users = new[]
            {
                new User
                {
                    FullName = "Admin",
                    Email = "admin@workmanagement.com",
                    Role = (int)UserRole.Admin,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new User
                {
                    FullName = "Manager",
                    Email = "manager@workmanagement.com",
                    Role = (int)UserRole.Manager,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new User
                {
                    FullName = "User1",
                    Email = "user1@workmanagement.com",
                    Role = (int)UserRole.User,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new User
                {
                    FullName = "User2",
                    Email = "user2@workmanagement.com",
                    Role = (int)UserRole.User,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new User
                {
                    FullName = "User3",
                    Email = "inactive@workmanagement.com",
                    Role = (int)UserRole.User,
                    IsActive = false,
                    CreatedAt = DateTime.Now.AddMonths(-6)
                }
            };

            context.Users.AddRange(users);
            context.SaveChanges();
            if (context.Tasks.Any())
            {
                return;
            }
            // Tasks
            var tasks = new[]
            {
                new Core.Entities.Task
                {
                    Title = "Thiết kế database cho hệ thống",
                    Description = "Tạo ERD và thiết kế các bảng cho hệ thống quản lý công việc",
                    Priority = (int)TaskPriority.High,
                    Status = (int)StatusTask.Completed,
                    Deadline = DateTime.Now.AddDays(-5),
                    AssignedTo = 3, // Lê Văn User
                    CreatedBy = 1, // Admin
                    CreatedAt = DateTime.Now.AddDays(-10)
                },
                new Core.Entities.Task
                {
                    Title = "Implement Authentication",
                    Description = "Xây dựng chức năng đăng nhập, đăng ký với ASP.NET Identity",
                    Priority = (int)TaskPriority.High,
                    Status = (int)StatusTask.InProgress,
                    Deadline = DateTime.Now.AddDays(3),
                    AssignedTo = 3,
                    CreatedBy = 2, // Manager
                    CreatedAt = DateTime.Now.AddDays(-7)
                },
                new Core.Entities.Task
                {
                    Title = "Tạo giao diện Dashboard",
                    Description = "Thiết kế và code giao diện dashboard với Bootstrap 5",
                    Priority = (int)TaskPriority.Medium,
                    Status = (int)StatusTask.Pending,
                    Deadline = DateTime.Now.AddDays(7),
                    AssignedTo = 4, // Phạm Thị User 2
                    CreatedBy = 2,
                    CreatedAt = DateTime.Now.AddDays(-3)
                },
                new Core.Entities.Task
                {
                    Title = "API Documentation",
                    Description = "Viết tài liệu API với Swagger",
                    Priority = (int)TaskPriority.Low,
                    Status = (int)StatusTask.Pending,
                    Deadline = DateTime.Now.AddDays(14),
                    AssignedTo = 3,
                    CreatedBy = 1,
                    CreatedAt = DateTime.Now.AddDays(-2)
                },
                new Core.Entities.Task
                {
                    Title = "Testing hệ thống",
                    Description = "Viết unit test và integration test",
                    Priority = (int)TaskPriority.High,
                    Status = (int)StatusTask.Pending,
                    Deadline = DateTime.Now.AddDays(10),
                    AssignedTo = 4,
                    CreatedBy = 2,
                    CreatedAt = DateTime.Now.AddDays(-1)
                },
                new Core.Entities.Task
                {
                    Title = "Deploy lên Production",
                    Description = "Deploy ứng dụng lên Azure/AWS",
                    Priority = (int)TaskPriority.Medium,
                    Status = (int)StatusTask.Pending,
                    Deadline = DateTime.Now.AddDays(21),
                    AssignedTo = 3,
                    CreatedBy = 1,
                    CreatedAt = DateTime.Now
                }
            };

            context.Tasks.AddRange(tasks);
            context.SaveChanges();
            if (context.Notifications.Any())
            {
                return;
            }
            // 3. Seed Notifications
            var notifications = new[]
            {
                new Notification
                {
                    UserId = 3,
                    TaskId = 1,
                    Type = (int)NotificationType.TaskAssigned,
                    Title = "Bạn được giao task: Thiết kế database cho hệ thống",
                    Channel = (int)NotificationChannel.InApp,
                    IsRead = true,
                    IsSent = true,
                    CreatedAt = DateTime.Now.AddDays(-10),
                    SentAt = DateTime.Now.AddDays(-10)
                },
                new Notification
                {
                    UserId = 3,
                    TaskId = 2,
                    Type = (int)NotificationType.TaskAssigned,
                    Title = "Bạn được giao task: Implement Authentication",
                    Channel = (int)NotificationChannel.InApp,
                    IsRead = true,
                    IsSent = true,
                    CreatedAt = DateTime.Now.AddDays(-7),
                    SentAt = DateTime.Now.AddDays(-7)
                },
                new Notification
                {
                    UserId = 3,
                    TaskId = 2,
                    Type = (int)NotificationType.DeadlineReminder,
                    Title = "Task 'Implement Authentication' sắp đến hạn (còn 3 ngày)",
                    Channel = (int)NotificationChannel.Email,
                    IsRead = false,
                    IsSent = true,
                    CreatedAt = DateTime.Now.AddHours(-2),
                    SentAt = DateTime.Now.AddHours(-2)
                },
                new Notification
                {
                    UserId = 4,
                    TaskId = 3,
                    Type = (int)NotificationType.TaskAssigned,
                    Title = "Bạn được giao task: Tạo giao diện Dashboard",
                    Channel = (int)NotificationChannel.InApp,
                    IsRead = false,
                    IsSent = true,
                    CreatedAt = DateTime.Now.AddDays(-3),
                    SentAt = DateTime.Now.AddDays(-3)
                },
                new Notification
                {
                    UserId = 3,
                    TaskId = 4,
                    Type = (int)NotificationType.TaskAssigned,
                    Title = "Bạn được giao task: API Documentation",
                    Channel = (int)NotificationChannel.InApp,
                    IsRead = false,
                    IsSent = true,
                    CreatedAt = DateTime.Now.AddDays(-2),
                    SentAt = DateTime.Now.AddDays(-2)
                },
                new Notification
                {
                    UserId = 4,
                    TaskId = 5,
                    Type = (int)NotificationType.TaskAssigned,
                    Title = "Bạn được giao task: Testing hệ thống",
                    Channel = (int)NotificationChannel.Email,
                    IsRead = false,
                    IsSent = false,
                    CreatedAt = DateTime.Now.AddDays(-1)
                },
                new Notification
                {
                    UserId = 3,
                    TaskId = 6,
                    Type = (int)NotificationType.TaskAssigned,
                    Title = "Bạn được giao task: Deploy lên Production",
                    Channel = (int)NotificationChannel.InApp,
                    IsRead = false,
                    IsSent = true,
                    CreatedAt = DateTime.Now,
                    SentAt = DateTime.Now
                }
            };

            context.Notifications.AddRange(notifications);
            context.SaveChanges();
        }
    }
}
