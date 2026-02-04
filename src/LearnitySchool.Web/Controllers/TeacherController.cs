using LearnitySchool.Application.Common;
using LearnitySchool.Infrastructure.Identity;
using LearnitySchool.Web.ViewModels.Teacher;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LearnitySchool.Web.Controllers;

[Authorize(Roles = RoleNames.Teacher)]
public class TeacherController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public TeacherController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public IActionResult Index() => View();

    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return RedirectToAction("Login", "Account");

        var vm = new TeacherProfileVm
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? "",
            UserName = user.UserName ?? ""
        };

        return View(vm);
    }
}
