using LearnitySchool.Web.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearnitySchool.Web.Controllers;

[Authorize(Roles = RoleNames.Teacher)]
public class TeacherController : Controller
{
    public IActionResult Index() => View();
}
