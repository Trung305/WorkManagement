using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkManagement.Core.Interfaces.Services;

namespace WorkManagement.Web.Controllers
{
    [Authorize]
    [Route("Stats")]
    public class StatsController : Controller
    {
        private readonly IDashboardService _dashboardService;

        private int CurrentUserId
        {
            get
            {
                var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return int.TryParse(value, out var id) ? id : 0;
            }
        }

        private int CurrentUserRole =>
            int.Parse(User.FindFirstValue(ClaimTypes.Role) ?? "3");

        public StatsController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("Index")]
        public async Task<IActionResult> Index(string? range = "30d", DateTime? from = null, DateTime? to = null)
        {
            var now = DateTime.Now;
            if (from == null && to == null)
            {
                from = range switch
                {
                    "7d" => now.AddDays(-7),
                    "30d" => now.AddDays(-30),
                    "90d" => now.AddDays(-90),
                    "thisMonth" => new DateTime(now.Year, now.Month, 1),
                    "lastMonth" => new DateTime(now.Year, now.Month, 1).AddMonths(-1),
                    "all" => null,
                    _ => now.AddDays(-30)
                };
                to = range == "lastMonth"
                    ? new DateTime(now.Year, now.Month, 1).AddDays(-1)
                    : now;
            }

            var stats = await _dashboardService.GetDetailedStatsAsync(
                CurrentUserId, CurrentUserRole, from, to);

            ViewBag.Range = range;
            ViewBag.From = from?.ToString("yyyy-MM-dd");
            ViewBag.To = to?.ToString("yyyy-MM-dd");

            return View(stats);
        }
    }
}
