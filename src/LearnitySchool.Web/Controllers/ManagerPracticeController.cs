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
        if (info.Type != LessonTaskType.Practice)
            return BadRequest("Це не Practice-задача.");

        var practice = await _db.PracticeTasks
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.LessonTaskId == taskId);

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
            StarterJs = practice?.StarterJs ?? "",

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
        // ✅ Вимикаємо валідацію “JS required”, навіть якщо десь стоїть [Required]
        ModelState.Remove(nameof(vm.ReferenceJs));
        ModelState.Remove(nameof(vm.StarterJs));

        // (опційно) якщо захочеш зробити і HTML/CSS не обов'язковими — розкоментуй:
        // ModelState.Remove(nameof(vm.ReferenceHtml));
        // ModelState.Remove(nameof(vm.ReferenceCss));
        // ModelState.Remove(nameof(vm.StarterHtml));
        // ModelState.Remove(nameof(vm.StarterCss));

        if (!ModelState.IsValid)
            return View(vm);

        var task = await _db.LessonTasks
            .FirstOrDefaultAsync(t => t.Id == vm.TaskId);

        if (task == null) return NotFound();
        if (task.Type != LessonTaskType.Practice)
            return BadRequest("Це не Practice-задача.");

        var practice = await _db.PracticeTasks
            .FirstOrDefaultAsync(x => x.LessonTaskId == vm.TaskId);

        if (practice == null)
        {
            practice = new PracticeTask
            {
                Id = Guid.NewGuid(),
                LessonTaskId = vm.TaskId
            };
            _db.PracticeTasks.Add(practice);
        }

        // ✅ Зберігаємо все як є — порожні значення ОК
        practice.Statement = vm.Statement?.Trim() ?? "";
        practice.SimilarityThreshold = vm.SimilarityThreshold;

        practice.StarterHtml = vm.StarterHtml ?? "";
        practice.StarterCss = vm.StarterCss ?? "";
        practice.StarterJs = vm.StarterJs ?? ""; // може бути пусто

        practice.ReferenceHtml = vm.ReferenceHtml ?? "";
        practice.ReferenceCss = vm.ReferenceCss ?? "";
        practice.ReferenceJs = vm.ReferenceJs ?? ""; // може бути пусто

        await _db.SaveChangesAsync();

        TempData["Success"] = "Practice content saved ✅";
        return RedirectToAction(nameof(Edit), new { taskId = vm.TaskId });
    }
}
