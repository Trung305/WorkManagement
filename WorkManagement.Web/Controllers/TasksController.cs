using AuthSystem.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using WorkManagement.Core.DTOs.Task;
using WorkManagement.Core.DTOs.User;
using WorkManagement.Core.Enums;
using WorkManagement.Core.Interfaces.Repositories;
using WorkManagement.Core.Interfaces.Services;
using WorkManagement.Web.Models.Task;

namespace WorkManagement.Web.Controllers
{
    [Authorize]
    [Route("Tasks")]
    public class TasksController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly ILogger<TasksController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public TasksController(ITaskService taskService, ILogger<TasksController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _taskService = taskService;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        private int CurrentUserId
        {
            get
            {
                var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _logger.LogInformation("CurrentUserId claim value: {Value}", value);
                return int.TryParse(value, out var id) ? id : 0;
            }
        }

        private int CurrentUserRole =>
            int.Parse(User.FindFirstValue(ClaimTypes.Role) ?? "3");

        private bool IsManager => CurrentUserRole == 2;
        private bool IsAdmin => CurrentUserRole == 1;
        private bool IsUser => CurrentUserRole == 3;

        // ── GET /Tasks/Index ─────────────────────────────────
        [HttpGet("Index")]
        public async Task<IActionResult> Index(
            int page = 1, string? q = null, int? status = null,
            int? priority = null, int? assignedTo = null,
            string view = "table", string? deadline = null)
        {
            DateTime? deadlineDate = null;
            if (!string.IsNullOrEmpty(deadline) && DateTime.TryParse(deadline, out var d))
                deadlineDate = d;

            var pagedResult = await _taskService.GetPagedAsync(
                page, 12, q, status, priority,
                assignedTo, CurrentUserId, CurrentUserRole, deadlineDate);

            var usersResult = IsUser
                ? Result<List<UserListDto>>.Success(new())
                : await _taskService.GetAssignableUsersAsync();

            var vm = new TaskIndexViewModel
            {
                PagedResult = pagedResult.Data!,
                SearchQuery = q,
                StatusFilter = status,
                PriorityFilter = priority,
                AssignedToFilter = assignedTo,
                ViewMode = view,
                Users = usersResult.Data ?? new(),
                ViewerRole = CurrentUserRole
            };

            return View("~/Views/Tasks/Index.cshtml", vm);
        }

        // ── GET /Tasks/create ────────────────────────────────
        [HttpGet("create")]
        public async Task<IActionResult> Create()
        {
            if (!IsManager && !IsAdmin) return Forbid();
            var u = await _taskService.GetAssignableUsersAsync();
            var vm = new TaskFormViewModel
            {
                AssignableUsers = u.Data ?? new(),
                Priority = 1
            };
            return View(vm);
        }

        // ── POST /Tasks/create ───────────────────────────────
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskFormViewModel vm)
        {
            if (!IsManager && !IsAdmin) return Forbid();

            if (vm.StartDate.HasValue && vm.Deadline.HasValue && vm.Deadline <= vm.StartDate)
                ModelState.AddModelError(nameof(vm.Deadline), "Ngày kết thúc phải lớn hơn ngày bắt đầu.");

            if (vm.Deadline.HasValue && vm.Deadline < DateTime.Now)
                ModelState.AddModelError(nameof(vm.Deadline), "Ngày kết thúc không được ở trong quá khứ.");

            if (!ModelState.IsValid)
            {
                var u = await _taskService.GetAssignableUsersAsync();
                vm.AssignableUsers = u.Data ?? new();
                ViewBag.CurrentUserId = CurrentUserId;  
                ViewBag.CurrentUserRole = CurrentUserRole;
                return View(vm);
            }

            var result = await _taskService.CreateAsync(new CreateTaskDto
            {
                Title = vm.Title,
                Description = vm.Description,
                Priority = vm.Priority,
                StartDate = vm.StartDate,
                Deadline = vm.Deadline,
                AssignedToId = vm.AssignedToId,
                CreatedById = CurrentUserId
            });

            if (!result.IsSuccess)
            {
                _logger.LogWarning(
                    "CREATE_TASK_FAILED | RequestId: {RequestId} | UserId: {UserId} | Error: {Error}",
                    HttpContext.TraceIdentifier, CurrentUserId, result.ErrorMessage);
                ModelState.AddModelError("", result.ErrorMessage);
                var u = await _taskService.GetAssignableUsersAsync();
                vm.AssignableUsers = u.Data ?? new();
                ViewBag.CurrentUserId = CurrentUserId;
                ViewBag.CurrentUserRole = CurrentUserRole;
                return View(vm);
            }

            _logger.LogInformation(
                "CREATE_TASK | RequestId: {RequestId} | UserId: {UserId} | TaskId: {TaskId} | Title: {Title} | AssignedTo: {AssignedTo}",
                HttpContext.TraceIdentifier, CurrentUserId, result.Data, vm.Title, vm.AssignedToId);

            if (vm.Attachments != null && vm.Attachments.Any())
            {
                var fileData = vm.Attachments
                    .Select(f => (f.OpenReadStream(), f.FileName, f.Length))
                    .ToList();
                await _taskService.UploadFilesAsync(result.Data, CurrentUserId, CurrentUserRole, fileData);
                _logger.LogInformation(
                    "UPLOAD_FILES | RequestId: {RequestId} | UserId: {UserId} | TaskId: {TaskId} | FileCount: {Count}",
                    HttpContext.TraceIdentifier, CurrentUserId, result.Data, vm.Attachments.Count);
            }

            TempData["Toast"] = "Tạo công việc thành công.";
            return RedirectToAction(nameof(Index));
        }

        // ── GET /Tasks/{id}/edit ─────────────────────────────
        [HttpGet("{id:int}/edit")]
        public async Task<IActionResult> Edit(int id)
        {
            var taskResult = await _taskService.GetByIdAsync(id);
            if (!taskResult.IsSuccess) return NotFound();
            var task = taskResult.Data!;

            if (IsUser && task.AssignedToId != CurrentUserId) return Forbid();

            var usersResult = IsUser
                ? Result<List<UserListDto>>.Success(new())
                : await _taskService.GetAssignableUsersAsync();

            var vm = new TaskFormViewModel
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Priority = task.Priority,
                StartDate = task.StartDate,
                Deadline = task.Deadline,
                AssignedToId = task.AssignedToId,
                AssignableUsers = usersResult.Data ?? new(),
                Status = task.Status,
                AssignedToName = task.AssignedToName,
                RejectedReason = task.RejectedReason
            };
            ViewBag.CurrentUserId = CurrentUserId;
            ViewBag.CurrentUserRole = CurrentUserRole;
            return View(vm);
        }

        // ── POST /Tasks/{id}/edit ────────────────────────────
        [HttpPost("{id:int}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaskFormViewModel vm)
        {
            if (!IsManager && !IsAdmin) return Forbid();
            vm.Id = id;

            if (vm.StartDate.HasValue && vm.Deadline.HasValue && vm.Deadline <= vm.StartDate)
                ModelState.AddModelError(nameof(vm.Deadline), "Ngày kết thúc phải lớn hơn ngày bắt đầu.");

            if (vm.Deadline.HasValue && vm.Deadline < DateTime.Now)
                ModelState.AddModelError(nameof(vm.Deadline), "Ngày kết thúc không được ở trong quá khứ.");

            if (!ModelState.IsValid)
            {
                var taskResult = await _taskService.GetByIdAsync(id);
                vm.Status = taskResult.Data?.Status ?? 0;

                var u = await _taskService.GetAssignableUsersAsync();
                vm.AssignableUsers = u.Data ?? new();
                ViewBag.CurrentUserId = CurrentUserId;
                ViewBag.CurrentUserRole = CurrentUserRole;
                return View(vm);
            }

            var result = await _taskService.UpdateAsync(new UpdateTaskDto
            {
                Id = id,
                Title = vm.Title,
                Description = vm.Description,
                Priority = vm.Priority,
                StartDate = vm.StartDate,
                Deadline = vm.Deadline,
                AssignedToId = vm.AssignedToId
            });

            if (!result.IsSuccess)
            {
                _logger.LogWarning(
                    "UPDATE_TASK_FAILED | RequestId: {RequestId} | UserId: {UserId} | TaskId: {TaskId} | Error: {Error}",
                    HttpContext.TraceIdentifier, CurrentUserId, id, result.ErrorMessage);
                ModelState.AddModelError("", result.ErrorMessage);
                var u = await _taskService.GetAssignableUsersAsync();
                vm.AssignableUsers = u.Data ?? new();
                ViewBag.CurrentUserId = CurrentUserId;
                ViewBag.CurrentUserRole = CurrentUserRole;
                return View(vm);
            }

            _logger.LogInformation(
                "UPDATE_TASK | RequestId: {RequestId} | UserId: {UserId} | TaskId: {TaskId} | Title: {Title}",
                HttpContext.TraceIdentifier, CurrentUserId, id, vm.Title);

            if (vm.Attachments != null && vm.Attachments.Any())
            {
                var fileData = vm.Attachments
                    .Select(f => (f.OpenReadStream(), f.FileName, f.Length))
                    .ToList();
                await _taskService.UploadFilesAsync(id, CurrentUserId, CurrentUserRole, fileData);
                _logger.LogInformation(
                    "UPLOAD_FILES | RequestId: {RequestId} | UserId: {UserId} | TaskId: {TaskId} | FileCount: {Count}",
                    HttpContext.TraceIdentifier, CurrentUserId, id, vm.Attachments.Count);
            }

            TempData["Toast"] = "Cập nhật công việc thành công.";
            return RedirectToAction(nameof(Index));
        }

        // ── POST /Tasks/{id}/delete ──────────────────────────
        [HttpPost("{id:int}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _taskService.DeleteAsync(id, CurrentUserId, CurrentUserRole);

            if (result.IsSuccess)
            {
                _logger.LogInformation(
                    "DELETE_TASK | RequestId: {RequestId} | UserId: {UserId} | TaskId: {TaskId}",
                    HttpContext.TraceIdentifier, CurrentUserId, id);
                TempData["Toast"] = "Đã xóa công việc.";
            }
            else
            {
                _logger.LogWarning(
                    "DELETE_TASK_FAILED | RequestId: {RequestId} | UserId: {UserId} | TaskId: {TaskId} | Error: {Error}",
                    HttpContext.TraceIdentifier, CurrentUserId, id, result.ErrorMessage);
                TempData["ToastError"] = result.ErrorMessage;
            }

            return RedirectToAction(nameof(Index));
        }

        // ── POST /Tasks/{id}/status ──────────────────────────
        [HttpPost("{id:int}/status")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, int newStatus)
        {
            var result = await _taskService.UpdateStatusAsync(
                new UpdateTaskStatusDto { Id = id, NewStatus = newStatus },
                CurrentUserId, CurrentUserRole);

            if (result.IsSuccess)
            {
                _logger.LogInformation(
                    "UPDATE_STATUS | RequestId: {RequestId} | UserId: {UserId} | TaskId: {TaskId} | NewStatus: {Status}",
                    HttpContext.TraceIdentifier, CurrentUserId, id, newStatus);
                TempData["Toast"] = "Đã cập nhật trạng thái.";
            }
            else
            {
                _logger.LogWarning(
                    "UPDATE_STATUS_FAILED | RequestId: {RequestId} | UserId: {UserId} | TaskId: {TaskId} | Error: {Error}",
                    HttpContext.TraceIdentifier, CurrentUserId, id, result.ErrorMessage);
                TempData["ToastError"] = result.ErrorMessage;
            }

            return RedirectToAction(nameof(Index));
        }

        // ── GET /Tasks/{id}/review ───────────────────────────
        [HttpGet("{id:int}/review")]
        public async Task<IActionResult> Review(int id)
        {
            if (!IsManager && !IsAdmin) return Forbid();

            var taskResult = await _taskService.GetByIdAsync(id);
            if (!taskResult.IsSuccess) return NotFound();

            var vm = new ReviewTaskViewModel
            {
                TaskId = taskResult.Data!.Id,
                TaskTitle = taskResult.Data!.Title,
                Approved = true
            };

            return View(vm);
        }

        // ── POST /Tasks/{id}/review ──────────────────────────
        [HttpPost("{id:int}/review")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(int id, ReviewTaskViewModel vm)
        {
            if (!IsManager && !IsAdmin) return Forbid();

            if (!vm.Approved && string.IsNullOrWhiteSpace(vm.RejectedReason))
                ModelState.AddModelError("RejectedReason", "Vui lòng nhập lý do từ chối.");

            if (!ModelState.IsValid)
            {
                vm.TaskId = id;
                return View(vm);
            }

            var result = await _taskService.ReviewAsync(new ReviewTaskDto
            {
                Id = id,
                Approved = vm.Approved,
                RejectedReason = vm.RejectedReason,
                ReviewedById = CurrentUserId
            });

            if (result.IsSuccess)
            {
                _logger.LogInformation(
                    "REVIEW_TASK | RequestId: {RequestId} | UserId: {UserId} | TaskId: {TaskId} | Approved: {Approved} | Reason: {Reason}",
                    HttpContext.TraceIdentifier, CurrentUserId, id, vm.Approved,
                    vm.Approved ? null : vm.RejectedReason);
                TempData["Toast"] = vm.Approved ? "Đã duyệt công việc." : "Đã từ chối công việc.";
            }
            else
            {
                _logger.LogWarning(
                    "REVIEW_TASK_FAILED | RequestId: {RequestId} | UserId: {UserId} | TaskId: {TaskId} | Error: {Error}",
                    HttpContext.TraceIdentifier, CurrentUserId, id, result.ErrorMessage);
                TempData["ToastError"] = result.ErrorMessage;
            }

            return RedirectToAction(nameof(Index));
        }

        // ── GET /Tasks/{id}/files ────────────────────────────
        [HttpGet("{id:int}/files")]
        public async Task<IActionResult> GetFiles(int id)
        {
            var result = await _taskService.GetByIdAsync(id);
            if (!result.IsSuccess) return NotFound();
            if (IsUser && result.Data!.AssignedToId != CurrentUserId) return Forbid();

            var files = await _taskService.GetFilesAsync(id);
            return Json(files.Data);
        }

        // ── POST /Tasks/{id}/upload ──────────────────────────
        [HttpPost("{id:int}/upload")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadFiles(int id, List<IFormFile> files)
        {
            var taskResult = await _taskService.GetByIdAsync(id);
            if (!taskResult.IsSuccess) return NotFound();
            if (IsUser && taskResult.Data!.AssignedToId != CurrentUserId) return Forbid();

            if (files == null || !files.Any())
                return BadRequest("Không có file nào được chọn.");

            var fileData = files.Select(f => (f.OpenReadStream(), f.FileName, f.Length)).ToList();
            var result = await _taskService.UploadFilesAsync(id, CurrentUserId, CurrentUserRole, fileData);

            if (result.IsSuccess)
            {
                _logger.LogInformation(
                    "UPLOAD_FILES | RequestId: {RequestId} | UserId: {UserId} | TaskId: {TaskId} | FileCount: {Count}",
                    HttpContext.TraceIdentifier, CurrentUserId, id, files.Count);
                return Ok();
            }

            _logger.LogWarning(
                "UPLOAD_FILES_FAILED | RequestId: {RequestId} | UserId: {UserId} | TaskId: {TaskId} | Error: {Error}",
                HttpContext.TraceIdentifier, CurrentUserId, id, result.ErrorMessage);
            return BadRequest(result.ErrorMessage);
        }
        [HttpDelete("files/{fileId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFile(int fileId)
        {
            var result = await _taskService.DeleteFileAsync(
                fileId, CurrentUserId, CurrentUserRole,
                _webHostEnvironment.WebRootPath);

            if (!result.IsSuccess)
                return BadRequest(result.ErrorMessage);
            return Ok();
        }
        // ── POST /Tasks/{id}/submit ──────────────────────────
        [HttpPost("{id:int}/submit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int id)
        {
            var taskResult = await _taskService.GetByIdAsync(id);
            if (!taskResult.IsSuccess) return NotFound();
            if (IsUser && taskResult.Data!.AssignedToId != CurrentUserId) return Forbid();

            var result = await _taskService.SubmitForReviewAsync(id, CurrentUserId);

            if (result.IsSuccess)
            {
                _logger.LogInformation(
                    "SUBMIT_TASK | RequestId: {RequestId} | UserId: {UserId} | TaskId: {TaskId}",
                    HttpContext.TraceIdentifier, CurrentUserId, id);
                return Ok();
            }

            _logger.LogWarning(
                "SUBMIT_TASK_FAILED | RequestId: {RequestId} | UserId: {UserId} | TaskId: {TaskId} | Error: {Error}",
                HttpContext.TraceIdentifier, CurrentUserId, id, result.ErrorMessage);
            return BadRequest(result.ErrorMessage);
        }

        // ── GET /Tasks/download/{fileId} ─────────────────────
        [HttpGet("download/{fileId:int}")]
        public async Task<IActionResult> Download(int fileId)
        {
            var result = await _taskService.GetFileAsync(fileId);
            if (!result.IsSuccess) return NotFound();

            var file = result.Data!;
            var fullPath = Path.Combine("wwwroot", file.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (!System.IO.File.Exists(fullPath)) return NotFound();

            _logger.LogInformation(
                "DOWNLOAD_FILE | RequestId: {RequestId} | UserId: {UserId} | FileId: {FileId} | FileName: {FileName}",
                HttpContext.TraceIdentifier, CurrentUserId, fileId, file.FileName);

            var bytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            return File(bytes, "application/octet-stream", file.FileName);
        }
        [HttpPost("test-reminder")]
        public async Task<IActionResult> TestReminder()
        {
            using var scope = HttpContext.RequestServices.CreateScope();
            var taskRepo = scope.ServiceProvider.GetRequiredService<ITaskRepository>();
            var notifService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var tasks = await taskRepo.GetTasksNeedingReminderAsync();
            foreach (var task in tasks)
            {
                await notifService.AddAsync(
                    userId: task.AssignedTo,
                    taskId: task.Id,
                    type: NotificationType.DeadlineReminder,
                    title: $"⏰ [TEST] Task \"{task.Title}\" sắp đến hạn!",
                    channel: NotificationChannel.InApp,
                    reminderType: 1
                );
            }
            return Ok($"Đã gửi {tasks.Count} thông báo test.");
        }
    }
}