using AuthSystem.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkManagement.Core.DTOs.File;
using WorkManagement.Core.DTOs.Task;
using WorkManagement.Core.DTOs.User;

namespace WorkManagement.Core.Interfaces.Services
{
    public interface ITaskService
    {
        Task<Result<TaskPagedResultDto>> GetPagedAsync(
            int page, int pageSize,
            string? search, int? status, int? priority,
            int? assignedToId, int? viewerUserId, int? viewerRole, DateTime? deadlineDate);

        Task<Result<TaskListDto>> GetByIdAsync(int id);
        Task<Result<int>> CreateAsync(CreateTaskDto dto);
        Task<Result> UpdateAsync(UpdateTaskDto dto);
        Task<Result> DeleteAsync(int id, int requesterId, int requesterRole);
        Task<Result> UpdateStatusAsync(UpdateTaskStatusDto dto, int requesterId, int requesterRole);
        Task<Result> ReviewAsync(ReviewTaskDto dto);
        Task<Result<List<UserListDto>>> GetAssignableUsersAsync();
        Task<Result<List<FileAttachmentDto>>> GetFilesAsync(int taskId);
        Task<Result> UploadFilesAsync(int taskId, int uploadedBy, int uploadedByRole, List<(Stream stream, string fileName, long fileSize)> files);
        Task<Result> SubmitForReviewAsync(int taskId, int userId);
        Task<Result<FileAttachmentDto>> GetFileAsync(int fileId);
    }
}
