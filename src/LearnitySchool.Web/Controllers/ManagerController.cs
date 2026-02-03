using LearnitySchool.Web.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearnitySchool.Web.Controllers;

[Authorize(Roles = RoleNames.Manager)]
public class ManagerController : Controller
{
    public IActionResult Index() => View();
}
