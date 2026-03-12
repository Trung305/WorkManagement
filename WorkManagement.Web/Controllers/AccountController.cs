using Microsoft.AspNetCore.Mvc;

namespace WorkManagement.Web.Controllers
{
    public class AccountController : Controller
    {
        // Hiển thị trang Login
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

            // Xử lý đăng nhập
            [HttpPost]
        public IActionResult Login(string email, string password)
        {
            if (email == "admin@gmail.com" && password == "123")
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Sai email hoặc mật khẩu";
            return View();
        }
    }
}