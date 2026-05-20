using LearnitySchool.Application.Common;
using LearnitySchool.Domain.Entities;
using LearnitySchool.Infrastructure.Identity;
using LearnitySchool.Infrastructure.Persistence;
using LearnitySchool.Web.ViewModels.Student.Projects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearnitySchool.Web.Controllers;

[Authorize(Roles = RoleNames.Student)]
public class StudentProjectsController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public StudentProjectsController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return RedirectToAction("Login", "Account");

        var vm = new StudentProjectListVm
        {
            Items = await _db.StudentProjects
                .AsNoTracking()
                .Where(p => p.StudentId == userId)
                .OrderByDescending(p => p.UpdatedAtUtc)
                .Select(p => new StudentProjectRowVm
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    CreatedAtUtc = p.CreatedAtUtc,
                    UpdatedAtUtc = p.UpdatedAtUtc
                })
                .ToListAsync()
        };

        return View(vm);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View("Edit", new StudentProjectEditVm
        {
            Title = "Новий проєкт",
            Html = "<h1>Hello Learnity</h1>\n<p>Мій перший тренувальний проєкт.</p>",
            Css = "body {\n  font-family: Arial, sans-serif;\n  padding: 24px;\n}\nh1 {\n  color: #2563EB;\n}",
            Js = "console.log('Hello Learnity');",
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return RedirectToAction("Login", "Account");

        var project = await _db.StudentProjects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id && p.StudentId == userId);
        if (project == null) return NotFound();

        return View(new StudentProjectEditVm
        {
            Id = project.Id,
            Title = project.Title,
            Description = project.Description,
            Html = project.Html,
            Css = project.Css,
            Js = project.Js,
            CreatedAtUtc = project.CreatedAtUtc,
            UpdatedAtUtc = project.UpdatedAtUtc
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(StudentProjectEditVm vm)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return RedirectToAction("Login", "Account");

        if (!ModelState.IsValid) return View("Edit", vm);

        StudentProject? project;
        if (vm.Id == Guid.Empty)
        {
            project = new StudentProject
            {
                Id = Guid.NewGuid(),
                StudentId = userId,
                CreatedAtUtc = DateTime.UtcNow
            };
            _db.StudentProjects.Add(project);
        }
        else
        {
            project = await _db.StudentProjects.FirstOrDefaultAsync(p => p.Id == vm.Id && p.StudentId == userId);
            if (project == null) return NotFound();
        }

        project.Title = vm.Title.Trim();
        project.Description = vm.Description?.Trim();
        project.Html = vm.Html ?? string.Empty;
        project.Css = vm.Css ?? string.Empty;
        project.Js = vm.Js ?? string.Empty;
        project.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        TempData["Success"] = "Проєкт збережено ✅";
        return RedirectToAction(nameof(Edit), new { id = project.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveAjax([FromBody] StudentProjectSaveVm vm)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        StudentProject? project;
        if (vm.Id == Guid.Empty)
        {
            project = new StudentProject
            {
                Id = Guid.NewGuid(),
                StudentId = userId,
                CreatedAtUtc = DateTime.UtcNow
            };
            _db.StudentProjects.Add(project);
        }
        else
        {
            project = await _db.StudentProjects.FirstOrDefaultAsync(p => p.Id == vm.Id && p.StudentId == userId);
            if (project == null) return NotFound();
        }

        project.Title = string.IsNullOrWhiteSpace(vm.Title) ? "Без назви" : vm.Title.Trim();
        project.Description = vm.Description?.Trim();
        project.Html = vm.Html ?? string.Empty;
        project.Css = vm.Css ?? string.Empty;
        project.Js = vm.Js ?? string.Empty;
        project.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Json(new { ok = true, id = project.Id, updatedAt = project.UpdatedAtUtc });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return RedirectToAction("Login", "Account");

        var project = await _db.StudentProjects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id && p.StudentId == userId);
        if (project == null) return NotFound();

        return View(project);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return RedirectToAction("Login", "Account");

        var project = await _db.StudentProjects.FirstOrDefaultAsync(p => p.Id == id && p.StudentId == userId);
        if (project == null) return NotFound();

        _db.StudentProjects.Remove(project);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Проєкт видалено.";
        return RedirectToAction(nameof(Index));
    }
}
