using LearnitySchool.Infrastructure.Identity;
using LearnitySchool.Application.Common;
using LearnitySchool.Web.ViewModels.Manager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LearnitySchool.Web.Controllers;

[Authorize(Roles = RoleNames.Manager)]
public class ManagerController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ManagerController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public IActionResult Index() => View();

    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return RedirectToAction("Login", "Account");

        var vm = new ManagerProfileVm
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? "",
            UserName = user.UserName ?? ""
        };

        return View(vm);
    }
}
