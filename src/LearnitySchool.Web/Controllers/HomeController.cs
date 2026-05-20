using LearnitySchool.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearnitySchool.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User?.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login", "Account");

        if (User.IsInRole(RoleNames.Manager)) return RedirectToAction("Index", "Manager");
        if (User.IsInRole(RoleNames.Teacher)) return RedirectToAction("Dashboard", "Teacher");
        return RedirectToAction("Index", "Student");
    }
}
