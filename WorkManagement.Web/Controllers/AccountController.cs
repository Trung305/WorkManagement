using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkManagement.Core.DTOs.Auth;
using WorkManagement.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace WorkManagement.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<TasksController> _logger;
        public AccountController(IAuthService authService, ILogger<TasksController> logger)
        {
            _authService = authService;
            _logger = logger;
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
        // GET: Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            // Nếu đã đăng nhập rồi thì redirect về Home
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");
            return View();
        }

        // POST: Account/Login
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
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddMinutes(60)
            });
            Response.Cookies.Append("refresh_token", result.Data.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");
            return View();
        }

        // POST: Account/Register
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
        [HttpGet("login-google")]
        [AllowAnonymous]
        public IActionResult LoginGoogle(string? returnUrl = "/Home/Index")
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleCallback", "Account", new { returnUrl })  
            };
            var redirectUri = Url.Action("GoogleCallback", new { returnUrl });
            _logger.LogInformation("Google RedirectUri: {Uri}", redirectUri);
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        // Callback sau khi Google xác thực xong
        [HttpGet("google-callback")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleCallback(string? returnUrl = "/Home/Index")
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                TempData["ToastError"] = "Đăng nhập Google thất bại.";
                return RedirectToAction("Login");
            }

            var email = result.Principal.FindFirstValue(ClaimTypes.Email)!;
            var googleId = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var fullName = result.Principal.FindFirstValue(ClaimTypes.Name) ?? email;

            var loginResult = await _authService.LoginWithGoogleAsync(email, googleId, fullName);
            if (!loginResult.IsSuccess)
            {
                TempData["ToastError"] = loginResult.ErrorMessage;
                return RedirectToAction("Login");
            }

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddMinutes(60)
            };
            var refreshOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            };

            Response.Cookies.Append("access_token", loginResult.Data!.AccessToken, cookieOptions);
            Response.Cookies.Append("refresh_token", loginResult.Data!.RefreshToken, refreshOptions);

            _logger.LogInformation(
    "LOGIN_GOOGLE | RequestId: {RequestId} | Email: {Email}",
    HttpContext.TraceIdentifier, email);

            return LocalRedirect(returnUrl ?? "/Home/Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddDays(-1)
            };
            Response.Cookies.Delete("access_token", options);
            Response.Cookies.Delete("refresh_token", options);

            // Xóa cookie Google OAuth scheme
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            _logger.LogInformation("LOGOUT | RequestId: {RequestId} | UserId: {UserId}",
                HttpContext.TraceIdentifier, CurrentUserId);

            return RedirectToAction("Login");
        }
    }
}