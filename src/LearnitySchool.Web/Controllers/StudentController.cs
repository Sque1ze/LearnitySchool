using LearnitySchool.Web.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearnitySchool.Web.Controllers;

[Authorize(Roles = RoleNames.Student)]
public class StudentController : Controller
{
    public IActionResult Index() => View();
}
