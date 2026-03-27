using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkManagement.Core.DTOs.User;
using WorkManagement.Core.Interfaces.Services;

namespace WorkManagement.Web.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<ProfileController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProfileController(IUserService userService, IWebHostEnvironment webHostEnvironment, ILogger<ProfileController> logger)
        {
            _userService = userService;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        private int CurrentUserId
        {
            get
            {
                var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return int.TryParse(value, out var id) ? id : 0;
            }
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await _userService.GetByIdAsync(CurrentUserId);
            if (!result.IsSuccess) return NotFound();
            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateProfileDto dto, IFormFile? avatar)
        {
            dto.Id = CurrentUserId;

            if (!ModelState.IsValid)
            {
                var u = await _userService.GetByIdAsync(CurrentUserId);
                return View("Index", u.Data);
            }

            // Upload avatar nếu có
            if (avatar != null && avatar.Length > 0)
            {
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var ext = Path.GetExtension(avatar.FileName).ToLower();
                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("", "Chỉ chấp nhận JPG, PNG, WEBP.");
                    var u = await _userService.GetByIdAsync(CurrentUserId);
                    return View("Index", u.Data);
                }
                if (avatar.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError("", "Ảnh không được vượt quá 2MB.");
                    var u = await _userService.GetByIdAsync(CurrentUserId);
                    return View("Index", u.Data);
                }

                var avatarResult = await _userService.UpdateAvatarAsync(
                    CurrentUserId, avatar.OpenReadStream(), avatar.FileName, _webHostEnvironment.WebRootPath);
                if (!avatarResult.IsSuccess)
                {
                    ModelState.AddModelError("", avatarResult.ErrorMessage);
                    var u = await _userService.GetByIdAsync(CurrentUserId);
                    return View("Index", u.Data);
                }
            }

            var result = await _userService.UpdateProfileAsync(dto);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.ErrorMessage);
                var u = await _userService.GetByIdAsync(CurrentUserId);
                return View("Index", u.Data);
            }

            TempData["Toast"] = "Cập nhật thông tin thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
