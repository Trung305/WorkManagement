using Microsoft.AspNetCore.Mvc;

namespace WorkManagement.Web.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
