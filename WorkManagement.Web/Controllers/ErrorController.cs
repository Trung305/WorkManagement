using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WorkManagement.Web.Controllers
{
    public class ErrorController : Controller
    {
        [AllowAnonymous]
        public IActionResult AccessDenied() => View();

        [AllowAnonymous]
        public IActionResult NotFound() => View();
    }
}
