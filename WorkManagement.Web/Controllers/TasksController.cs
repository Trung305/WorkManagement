using Microsoft.AspNetCore.Mvc;

namespace WorkManagement.Web.Controllers
{
    public class TasksController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
