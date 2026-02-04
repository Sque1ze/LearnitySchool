using LearnitySchool.Domain.Entities;
using LearnitySchool.Infrastructure.Persistence;
using LearnitySchool.Web.Common;
using LearnitySchool.Web.ViewModels.Manager.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearnitySchool.Web.Controllers;

[Authorize(Roles = RoleNames.Manager)]
public class ManagerTasksController : Controller
{
    private readonly AppDbContext _db;

    public ManagerTasksController(AppDbContext db)
    {
        _db = db;
    }

    // GET: /ManagerTasks?lessonId=...
    public async Task<IActionResult> Index(Guid lessonId)
    {
        var lessonTitle = await _db.Lessons
            .Where(x => x.Id == lessonId)
            .Select(x => x.Title)
            .FirstOrDefaultAsync();

        if (lessonTitle == null) return NotFound();

        ViewBag.LessonId = lessonId;
        ViewBag.LessonTitle = lessonTitle;

        var tasks = await _db.LessonTasks
            .Where(x => x.LessonId == lessonId)
            .OrderBy(x => x.Order)
            .Select(x => new TaskListItemVm
            {
                Id = x.Id,
                Order = x.Order,
                Title = x.Title,
                Type = x.Type,
                IsPublished = x.IsPublished
            })
            .ToListAsync();

        return View(tasks);
    }

    // GET: /ManagerTasks/Create?lessonId=...
    public async Task<IActionResult> Create(Guid lessonId)
    {
        var lessonTitle = await _db.Lessons
            .Where(x => x.Id == lessonId)
            .Select(x => x.Title)
            .FirstOrDefaultAsync();

        if (lessonTitle == null) return NotFound();

        var lastOrder = await _db.LessonTasks
            .Where(x => x.LessonId == lessonId)
            .MaxAsync(x => (int?)x.Order) ?? 0;

        var vm = new TaskEditVm
        {
            LessonId = lessonId,
            LessonTitle = lessonTitle,
            Order = lastOrder + 1
        };

        return View(vm);
    }

    // POST: /ManagerTasks/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TaskEditVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var orderExists = await _db.LessonTasks.AnyAsync(x =>
            x.LessonId == vm.LessonId && x.Order == vm.Order);

        if (orderExists)
        {
            ModelState.AddModelError(nameof(vm.Order), "Такий Order вже існує в цьому уроці.");
            return View(vm);
        }

        var task = new LessonTask
        {
            Id = Guid.NewGuid(),
            LessonId = vm.LessonId,
            Order = vm.Order,
            Title = vm.Title.Trim(),
            Description = vm.Description?.Trim(),
            Type = vm.Type,
            IsPublished = vm.IsPublished
        };

        _db.LessonTasks.Add(task);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { lessonId = vm.LessonId });
    }

    // GET: /ManagerTasks/Edit/{id}
    public async Task<IActionResult> Edit(Guid id)
    {
        var task = await _db.LessonTasks.FirstOrDefaultAsync(x => x.Id == id);
        if (task == null) return NotFound();

        var lessonTitle = await _db.Lessons
            .Where(x => x.Id == task.LessonId)
            .Select(x => x.Title)
            .FirstOrDefaultAsync() ?? "";

        return View(new TaskEditVm
        {
            Id = task.Id,
            LessonId = task.LessonId,
            LessonTitle = lessonTitle,
            Order = task.Order,
            Title = task.Title,
            Description = task.Description,
            Type = task.Type,
            IsPublished = task.IsPublished
        });
    }

    // POST: /ManagerTasks/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(TaskEditVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var task = await _db.LessonTasks.FirstOrDefaultAsync(x => x.Id == vm.Id);
        if (task == null) return NotFound();

        var orderExists = await _db.LessonTasks.AnyAsync(x =>
            x.LessonId == vm.LessonId &&
            x.Order == vm.Order &&
            x.Id != vm.Id);

        if (orderExists)
        {
            ModelState.AddModelError(nameof(vm.Order), "Такий Order вже існує в цьому уроці.");
            return View(vm);
        }

        task.Order = vm.Order;
        task.Title = vm.Title.Trim();
        task.Description = vm.Description?.Trim();
        task.Type = vm.Type;
        task.IsPublished = vm.IsPublished;

        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { lessonId = vm.LessonId });
    }

    // GET: /ManagerTasks/Delete/{id}
    public async Task<IActionResult> Delete(Guid id)
    {
        var task = await _db.LessonTasks.FirstOrDefaultAsync(x => x.Id == id);
        if (task == null) return NotFound();

        var lessonTitle = await _db.Lessons
            .Where(x => x.Id == task.LessonId)
            .Select(x => x.Title)
            .FirstOrDefaultAsync() ?? "";

        return View(new TaskEditVm
        {
            Id = task.Id,
            LessonId = task.LessonId,
            LessonTitle = lessonTitle,
            Order = task.Order,
            Title = task.Title
        });
    }

    // POST: /ManagerTasks/DeleteConfirmed
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id, Guid lessonId)
    {
        var task = await _db.LessonTasks.FirstOrDefaultAsync(x => x.Id == id);
        if (task == null) return NotFound();

        _db.LessonTasks.Remove(task);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { lessonId });
    }
}
