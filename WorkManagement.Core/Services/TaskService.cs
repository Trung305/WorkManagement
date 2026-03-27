using AuthSystem.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkManagement.Core.DTOs.Task;
using WorkManagement.Core.DTOs.User;
using WorkManagement.Core.Entities;
using WorkManagement.Core.Enums;
using WorkManagement.Core.Interfaces.Repositories;
using WorkManagement.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;
using WorkTask = WorkManagement.Core.Entities.Task;
using TaskStatus = WorkManagement.Core.Enums.TaskStatus;
using WorkManagement.Core.DTOs.File;
namespace WorkManagement.Core.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepo;
        private readonly IUserRepository _userRepo;
        private readonly INotificationRepository _notifRepo;
        private readonly ILogger<TaskService> _logger;
        public TaskService(
            ITaskRepository taskRepo,
            IUserRepository userRepo,
            INotificationRepository notifRepo,
            ILogger<TaskService> logger)
        {
            _taskRepo = taskRepo;
            _userRepo = userRepo;
            _notifRepo = notifRepo;
            _logger = logger;
        }

        public async Task<Result<TaskPagedResultDto>> GetPagedAsync(
    int page, int pageSize,
    string? search, int? status, int? priority,
    int? assignedToId, int? viewerUserId, int? viewerRole, DateTime? deadlineDate, DateTime? deadlineFrom = null, 
    DateTime? deadlineTo = null, 
    bool overdue = false)
        {
            var (items, total, stats) = await _taskRepo.GetPagedAsync(
                page, pageSize, search, status, priority,
                assignedToId, viewerUserId, viewerRole, deadlineDate, deadlineFrom, deadlineTo, overdue);

            var dto = new TaskPagedResultDto
            {
                Items = items.Select(MapToDto).ToList(),
                TotalCount = total,
                Page = page,
                PageSize = pageSize,
                CountPending = stats.GetValueOrDefault(1),
                CountInProgress = stats.GetValueOrDefault(2),
                CountPendingReview = stats.GetValueOrDefault(3),
                CountCompleted = stats.GetValueOrDefault(4),
                CountRejected = stats.GetValueOrDefault(5)
            };

            return Result<TaskPagedResultDto>.Success(dto);
        }

        public async Task<Result<TaskListDto>> GetByIdAsync(int id)
        {
            var task = await _taskRepo.GetByIdWithUsersAsync(id);
            if (task == null)
                return Result<TaskListDto>.Fail("Không tìm thấy công việc.");

            return Result<TaskListDto>.Success(MapToDto(task));
        }

        public async Task<Result<int>> CreateAsync(CreateTaskDto dto)
        {
            var task = new WorkTask
            {
                Title = dto.Title,
                Description = dto.Description,
                Priority = (TaskPriority)dto.Priority,
                Status = TaskStatus.Pending,
                StartDate = dto.StartDate,
                Deadline = dto.Deadline,
                AssignedTo = dto.AssignedToId,
                CreatedBy = dto.CreatedById,
                CreatedAt = DateTime.Now
            };

            await _taskRepo.AddAsync(task);

            await _notifRepo.AddAsync(new Notification
            {
                UserId = dto.AssignedToId,
                TaskId = task.Id,
                Type = NotificationType.TaskAssigned,
                Title = $"Bạn được giao task: {task.Title}",
                Channel = NotificationChannel.InApp,
                CreatedAt = DateTime.Now
            });

            _logger.LogInformation("Task {Id} created by {CreatedBy}", task.Id, dto.CreatedById);
            return Result<int>.Success(task.Id);
        }

        public async Task<Result> UpdateAsync(UpdateTaskDto dto)
        {
            var task = await _taskRepo.GetByIdAsync(dto.Id);
            if (task == null)
                return Result.Fail("Không tìm thấy công việc.");
            var oldAssignedTo = task.AssignedTo;
            task.Title = dto.Title;
            task.Description = dto.Description;
            task.Priority = (TaskPriority)dto.Priority;
            task.StartDate = dto.StartDate;
            task.Deadline = dto.Deadline;
            task.AssignedTo = dto.AssignedToId;
            task.UpdatedAt = DateTime.Now;

            await _taskRepo.UpdateAsync(task);
            if (dto.AssignedToId != oldAssignedTo)
            {
                await _notifRepo.DeleteByTaskAndUserAsync(task.Id, oldAssignedTo);

                var notification = new Notification
                {
                    UserId = dto.AssignedToId,
                    TaskId = task.Id,
                    Type = NotificationType.TaskAssigned,
                    Channel = NotificationChannel.InApp,
                    Title = $"Bạn được phân công task: {task.Title}",
                    IsRead = false,
                    IsSent = false,
                    CreatedAt = DateTime.Now
                };

                await _notifRepo.AddAsync(notification);
            }
            return Result.Success();
        }

        public async Task<Result> DeleteAsync(int id, int requesterId, int requesterRole)
        {
            var task = await _taskRepo.GetByIdAsync(id);
            if (task == null)
                return Result.Fail("Không tìm thấy công việc.");

            if (requesterRole != 0 && task.CreatedBy != requesterId)
                return Result.Fail("Bạn không có quyền xóa công việc này.");

            await _taskRepo.DeleteAsync(task);
            _logger.LogWarning("Task {Id} deleted by {UserId}", id, requesterId);
            return Result.Success();
        }

        public async Task<Result> UpdateStatusAsync(
            UpdateTaskStatusDto dto, int requesterId, int requesterRole)
        {
            var task = await _taskRepo.GetByIdAsync(dto.Id);
            if (task == null)
                return Result.Fail("Không tìm thấy công việc.");

            var current = (int)task.Status;
            var next = dto.NewStatus;

            task.Status = (TaskStatus)next;
            task.UpdatedAt = DateTime.Now;
            if (next == 2) task.StartedAt = DateTime.Now;

            await _taskRepo.UpdateAsync(task);

            if (next == 3 && task.CreatedBy > 0)
            {
                await _notifRepo.AddAsync(new Notification
                {
                    UserId = task.CreatedBy,
                    TaskId = task.Id,
                    Type = NotificationType.TaskStatusChanged,
                    Title = $"Task \"{task.Title}\" đang chờ đánh giá",
                    Channel = NotificationChannel.InApp,
                    CreatedAt = DateTime.Now
                });
            }

            return Result.Success();
        }

        public async Task<Result> ReviewAsync(ReviewTaskDto dto)
        {
            var task = await _taskRepo.GetByIdAsync(dto.Id);
            if (task == null)
                return Result.Fail("Không tìm thấy công việc.");

            if (task.Status != TaskStatus.PendingReview)
                return Result.Fail("Task không ở trạng thái chờ đánh giá.");

            task.ReviewedAt = DateTime.Now;
            task.UpdatedAt = DateTime.Now;

            if (dto.Approved)
            {
                task.Status = TaskStatus.Completed;
                task.CompletedAt = DateTime.Now;
            }
            else
            {
                task.Status = TaskStatus.Rejected;
                task.RejectedReason = dto.RejectedReason;
            }

            await _taskRepo.UpdateAsync(task);

            await _notifRepo.AddAsync(new Notification
            {
                UserId = task.AssignedTo,
                TaskId = task.Id,
                Type = dto.Approved ? NotificationType.TaskCompleted : NotificationType.TaskRejected,
                Title = dto.Approved
                                    ? $"Công việc \"{task.Title}\" đã được duyệt "
                                    : $"Công việc \"{task.Title}\" bị từ chối",
                Channel = NotificationChannel.InApp,
                CreatedAt = DateTime.Now
            });

            return Result.Success();
        }
        public async Task<Result> DeleteFileAsync(int fileId, int requesterId, int requesterRole, string webRootPath)
        {
            var file = await _taskRepo.GetFileByIdAsync(fileId);
            if (file == null)
                return Result.Fail("File không tồn tại.");

            if (requesterRole >= 3 && file.UploadedBy != requesterId)
                return Result.Fail("Bạn không có quyền xóa file này.");

            // Xóa file vật lý
            var fullPath = Path.Combine(webRootPath, file.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);

            await _taskRepo.DeleteFileAsync(file);
            return Result.Success();
        }
        public async Task<Result<List<UserListDto>>> GetAssignableUsersAsync()
        {
            var users = await _userRepo.GetByRoleAsync((int)UserRole.User);

            var list = users.Select(u => new UserListDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = (int)u.Role,
                IsActive = u.IsActive
            }).ToList();

            return Result<List<UserListDto>>.Success(list);
        }
        private static TaskListDto MapToDto(WorkTask t) => new()
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            Priority = (int)t.Priority,
            Status = (int)t.Status,
            StartDate = t.StartDate,
            Deadline = t.Deadline,
            CreatedAt = t.CreatedAt,
            StartedAt = t.StartedAt,
            CompletedAt = t.CompletedAt,
            AssignedToId = t.AssignedTo,
            AssignedToName = t.AssignedUser?.FullName,
            AssignedToAvatar = t.AssignedUser?.AvatarUrl,
            CreatedById = t.CreatedBy,
            CreatedByName = t.CreatedByUser?.FullName ?? "",
            RejectedReason = t.RejectedReason
        };
        public async Task<Result<List<FileAttachmentDto>>> GetFilesAsync(int taskId)
        {
            var files = await _taskRepo.GetFilesByTaskIdAsync(taskId);
            return Result<List<FileAttachmentDto>>.Success(files.Select(f => new FileAttachmentDto
            {
                Id = f.Id,
                FileName = f.FileName,
                FilePath = f.FilePath,
                FileSize = f.FileSize,
                UploadedByRole = f.UploadedByRole,
                UploadedByName = f.UploadedByUser?.FullName ?? "—",
                UploadedByUser = f.UploadedByUser?.Id ?? 0,
                UploadedAt = f.UploadedAt
            }).ToList());
        }

        public async Task<Result> UploadFilesAsync(int taskId, int uploadedBy, int uploadedByRole, List<(Stream stream, string fileName, long fileSize)> files)
        {
            foreach (var (stream, fileName, fileSize) in files)
            {
                if (fileSize > 5 * 1024 * 1024)
                    return Result.Fail($"File \"{fileName}\" vượt quá 5MB.");

                var ext = Path.GetExtension(fileName);
                var saved = $"{Guid.NewGuid()}{ext}";
                var folder = Path.Combine("wwwroot", "uploads", "tasks", taskId.ToString());
                Directory.CreateDirectory(folder);
                var fullPath = Path.Combine(folder, saved);

                using var fileStream = System.IO.File.Create(fullPath);
                await stream.CopyToAsync(fileStream);

                await _taskRepo.AddFileAsync(new FileAttachment
                {
                    TaskId = taskId,
                    FileName = fileName,
                    FilePath = $"/uploads/tasks/{taskId}/{saved}",
                    FileSize = fileSize,
                    UploadedBy = uploadedBy,
                    UploadedByRole = uploadedByRole,
                    UploadedAt = DateTime.Now
                });
            }
            return Result.Success();
        }

        public async Task<Result> SubmitForReviewAsync(int taskId, int userId)
        {
            return await UpdateStatusAsync(
                new UpdateTaskStatusDto { Id = taskId, NewStatus = 3 },
                userId, 3);
        }

        public async Task<Result<FileAttachmentDto>> GetFileAsync(int fileId)
        {
            var file = await _taskRepo.GetFileByIdAsync(fileId);
            if (file == null) return Result<FileAttachmentDto>.Fail("Không tìm thấy file.");
            return Result<FileAttachmentDto>.Success(new FileAttachmentDto
            {
                Id = file.Id,
                FileName = file.FileName,
                FilePath = file.FilePath,
                FileSize = file.FileSize,
                UploadedByRole = file.UploadedByRole,
                UploadedAt = file.UploadedAt
            });
        }
    }
}
