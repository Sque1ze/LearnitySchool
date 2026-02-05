using LearnitySchool.Application.Common;
using LearnitySchool.Domain.Entities;
using LearnitySchool.Domain.Enums;
using LearnitySchool.Infrastructure.Persistence;
using LearnitySchool.Web.ViewModels.Manager.Practice;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearnitySchool.Web.Controllers;

[Authorize(Roles = RoleNames.Manager)]
public class ManagerPracticeController : Controller
{
    private readonly AppDbContext _db;

    public ManagerPracticeController(AppDbContext db)
    {
        _db = db;
    }

    // GET: /ManagerPractice/Edit?taskId=...
    public async Task<IActionResult> Edit(Guid taskId)
    {
        var info = await _db.LessonTasks
            .Where(t => t.Id == taskId)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.Type,
                t.LessonId,
                CourseId = t.Lesson.CourseId
            })
            .FirstOrDefaultAsync();

        if (info == null) return NotFound();
        if (info.Type != LessonTaskType.Practice) return BadRequest("Це не Practice-задача.");

        var practice = await _db.PracticeTasks.FirstOrDefaultAsync(x => x.LessonTaskId == taskId);

        var vm = new PracticeEditVm
        {
            TaskId = taskId,
            LessonId = info.LessonId,
            CourseId = info.CourseId,
            TaskTitle = info.Title,

            Statement = practice?.Statement ?? "",
            SimilarityThreshold = practice?.SimilarityThreshold ?? 80,

            StarterHtml = practice?.StarterHtml ?? "<!-- write your HTML here -->",
            StarterCss = practice?.StarterCss ?? "/* write your CSS here */",
            StarterJs = practice?.StarterJs ?? "// write your JS here",

            ReferenceHtml = practice?.ReferenceHtml ?? "",
            ReferenceCss = practice?.ReferenceCss ?? "",
            ReferenceJs = practice?.ReferenceJs ?? ""
        };

        return View(vm);
    }

    // POST: /ManagerPractice/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PracticeEditVm vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var task = await _db.LessonTasks.FirstOrDefaultAsync(t => t.Id == vm.TaskId);
        if (task == null) return NotFound();
        if (task.Type != LessonTaskType.Practice) return BadRequest("Це не Practice-задача.");

        var practice = await _db.PracticeTasks.FirstOrDefaultAsync(x => x.LessonTaskId == vm.TaskId);

        if (practice == null)
        {
            practice = new PracticeTask
            {
                Id = Guid.NewGuid(),
                LessonTaskId = vm.TaskId
            };
            _db.PracticeTasks.Add(practice);
        }

        practice.Statement = vm.Statement?.Trim() ?? "";
        practice.SimilarityThreshold = vm.SimilarityThreshold;

        practice.StarterHtml = vm.StarterHtml ?? "";
        practice.StarterCss = vm.StarterCss ?? "";
        practice.StarterJs = vm.StarterJs ?? "";

        practice.ReferenceHtml = vm.ReferenceHtml ?? "";
        practice.ReferenceCss = vm.ReferenceCss ?? "";
        practice.ReferenceJs = vm.ReferenceJs ?? "";

        await _db.SaveChangesAsync();

        TempData["Success"] = "Practice content saved ✅";
        return RedirectToAction(nameof(Edit), new { taskId = vm.TaskId });
    }
}
