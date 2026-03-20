using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkManagement.Core.DTOs.User;
using WorkManagement.Core.Interfaces.Services;
using WorkManagement.Web.Models.Admin;

namespace WorkManagement.Web.Controllers;

[Authorize(Policy = "AdminOnly")]
[Route("/Users")]
public class AdminUsersController : Controller
{
    private readonly IUserService _userService;

    public AdminUsersController(IUserService userService)
    {
        _userService = userService;
    }

    // GET /admin/users
    [HttpGet("Index")]
    public async Task<IActionResult> Index(
        int page = 1, string? search = null, int? role = null, string? isActive = null)
    {
        bool? activeFilter = isActive switch { "true" => true, "false" => false, _ => null };

        var result = await _userService.GetPagedAsync(page, 10, search, role, activeFilter);

        ViewBag.Page = page;
        ViewBag.Search = search;
        ViewBag.Role = role;
        ViewBag.IsActive = isActive;

        var vm = new UserIndexViewModel
        {
            PagedResult = result.Data!,
            SearchQuery = search,
            RoleFilter = role,
            StatusFilter = activeFilter
        };

        return View("~/Views/Users/Index.cshtml", vm);
    }

    // POST /admin/users/create  (JSON)
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        var result = await _userService.CreateAsync(dto);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);
        return Ok();
    }

    // PUT /admin/users/{id}  (JSON)
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        dto.Id = id;
        var result = await _userService.UpdateAsync(dto);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);
        return Ok();
    }

    // POST /admin/users/{id}/toggle
    [HttpPost("{id:int}/toggle")]
    public async Task<IActionResult> Toggle(int id)
    {
        var result = await _userService.ToggleActiveAsync(id);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);
        return Ok();
    }

    // DELETE /admin/users/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _userService.DeleteAsync(id);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);
        return Ok();
    }
}