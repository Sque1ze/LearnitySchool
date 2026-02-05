using LearnitySchool.Application.Abstractions;
using LearnitySchool.Application.Common;
using LearnitySchool.Web.ViewModels.Groups;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearnitySchool.Web.Controllers;

[Authorize(Roles = RoleNames.Teacher)]
public class GroupsController : Controller
{
    private readonly IGroupsService _groups;

    public GroupsController(IGroupsService groups)
    {
        _groups = groups;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var list = await _groups.GetTeacherGroupsAsync(teacherId, ct);

        return View(new GroupsIndexVm { Groups = list });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["Error"] = "Назва групи не може бути порожньою.";
            return RedirectToAction(nameof(Index));
        }

        var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        await _groups.CreateGroupAsync(teacherId, name, ct);

        return RedirectToAction(nameof(Index));
    }
}
