using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using WorkManagement.Core.Interfaces.Services;

namespace WorkManagement.Web.Filters
{
    public class SetUserAvatarFilter : IAsyncActionFilter
    {
        private readonly IUserService _userService;
        public SetUserAvatarFilter(IUserService userService)
        {
            _userService = userService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = context.HttpContext.User;
            if (user.Identity?.IsAuthenticated == true)
            {
                var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(value, out var userId))
                {
                    var result = await _userService.GetByIdAsync(userId);
                    if (result.IsSuccess)
                    {
                        context.HttpContext.Items["AvatarUrl"] = result.Data!.AvatarUrl;
                        context.HttpContext.Items["FullName"] = result.Data!.FullName;
                    }
                }
            }
            await next();
        }
    }
}