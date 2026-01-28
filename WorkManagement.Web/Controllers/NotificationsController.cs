using Microsoft.AspNetCore.Mvc;

namespace WorkManagement.Web.Controllers
{
    public class NotificationsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
