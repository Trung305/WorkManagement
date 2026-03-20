using Microsoft.AspNetCore.Mvc;
using WorkManagement.Core.DTOs.Auth;
using WorkManagement.Core.Interfaces.Services;

namespace WorkManagement.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            // Nếu đã đăng nhập rồi thì redirect về Home
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await _authService.LoginAsync(dto);
            if (!result.IsSuccess)
            {
                ViewBag.Error = result.ErrorMessage;
                return View(dto);
            }

            // Lưu AccessToken vào cookie để dùng cho các request sau
            Response.Cookies.Append("access_token", result.Data!.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,     // đổi thành true khi deploy HTTPS
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddMinutes(60)
            });
            Response.Cookies.Append("refresh_token", result.Data.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await _authService.RegisterAsync(dto);
            if (!result.IsSuccess)
            {
                ViewBag.Error = result.ErrorMessage;
                return View(dto);
            }

            TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("access_token");
            Response.Cookies.Delete("refresh_token");
            return RedirectToAction("Login");
        }
    }
}