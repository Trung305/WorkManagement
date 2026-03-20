using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkManagement.Core.Interfaces.Services;
using WorkManagement.Web.Models;

namespace WorkManagement.Web.Controllers;
[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly INotificationService _notifService;
    private readonly IDashboardService _dashboardService;

    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub") ?? "0");

    private int CurrentUserRole =>
        int.Parse(User.FindFirstValue(ClaimTypes.Role) ?? "3");

    public HomeController(ILogger<HomeController> logger, IDashboardService dashboardService, INotificationService notifService)
    {
        _logger = logger;
        _dashboardService = dashboardService;
        _notifService = notifService;
    }

    public async Task<IActionResult> Index()
    {
        var stats = await _dashboardService.GetStatsAsync(CurrentUserId, CurrentUserRole);
        return View(stats);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    public async Task<IActionResult> Notifications()
    {
        var result = await _notifService.GetByUserIdAsync(CurrentUserId);
        return View(result.Data ?? new());
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(int id)
    {
        await _notifService.MarkAsReadAsync(id, CurrentUserId);
        return Ok();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead()
    {
        await _notifService.MarkAllAsReadAsync(CurrentUserId);
        return Ok();
    }
}
