using LearnitySchool.Web.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearnitySchool.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        // If authenticated, redirect to role dashboard
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole(RoleNames.Manager)) return RedirectToAction("Index", "Manager");
            if (User.IsInRole(RoleNames.Teacher)) return RedirectToAction("Index", "Teacher");
            if (User.IsInRole(RoleNames.Student)) return RedirectToAction("Index", "Student");
        }

        return View();
    }
}
