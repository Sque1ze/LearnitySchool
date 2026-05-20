using LearnitySchool.Infrastructure.Persistence;
using LearnitySchool.Application.Common;
using LearnitySchool.Web.ViewModels.Manager.Lessons;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearnitySchool.Web.Controllers;

[Authorize(Roles = RoleNames.Manager)]
public class ManagerLessonsController : Controller
{
    private readonly AppDbContext _db;

    public ManagerLessonsController(AppDbContext db)
    {
        _db = db;
    }

    // GET: /ManagerLessons?courseId=...
    public async Task<IActionResult> Index(Guid courseId)
    {
        var course = await _db.Courses.FirstOrDefaultAsync(x => x.Id == courseId);
        if (course == null) return NotFound();

        ViewBag.CourseId = courseId;
        ViewBag.CourseTitle = course.Title;

        var lessons = await _db.Lessons
            .Where(x => x.CourseId == courseId)
            .OrderBy(x => x.Order)
            .Select(x => new LessonListItemVm
            {
                Id = x.Id,
                CourseId = x.CourseId,
                Order = x.Order,
                Title = x.Title,
                IsPublished = x.IsPublished
            })
            .ToListAsync();

        return View(lessons);
    }

    // GET: /ManagerLessons/Create?courseId=...
    public async Task<IActionResult> Create(Guid courseId)
    {
        var course = await _db.Courses.FirstOrDefaultAsync(x => x.Id == courseId);
        if (course == null) return NotFound();

        // default Order = last + 1
        var lastOrder = await _db.Lessons
            .Where(x => x.CourseId == courseId)
            .MaxAsync(x => (int?)x.Order) ?? 0;

        var vm = new LessonEditVm
        {
            CourseId = courseId,
            CourseTitle = course.Title,
            Order = lastOrder + 1
        };

        return View(vm);
    }

    // POST: /ManagerLessons/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LessonEditVm vm)
    {
        if (!ModelState.IsValid)
        {
            vm.CourseTitle = await _db.Courses.Where(x => x.Id == vm.CourseId).Select(x => x.Title).FirstOrDefaultAsync() ?? string.Empty;
            return View(vm);
        }

        // унікальність Order в курсі
        var orderExists = await _db.Lessons.AnyAsync(x => x.CourseId == vm.CourseId && x.Order == vm.Order);
        if (orderExists)
        {
            ModelState.AddModelError(nameof(vm.Order), "Такий Order вже існує в цьому курсі.");
            vm.CourseTitle = await _db.Courses.Where(x => x.Id == vm.CourseId).Select(x => x.Title).FirstOrDefaultAsync() ?? string.Empty;
            return View(vm);
        }

        var lesson = new LearnitySchool.Domain.Entities.Lesson
        {
            Id = Guid.NewGuid(),
            CourseId = vm.CourseId,
            Order = vm.Order,
            Title = vm.Title.Trim(),
            Description = vm.Description?.Trim(),
            Content = vm.Content,
            IsPublished = vm.IsPublished,
            PriceAmount = vm.PriceAmount,
            Currency = string.IsNullOrWhiteSpace(vm.Currency) ? "UAH" : vm.Currency.Trim().ToUpperInvariant()
        };

        _db.Lessons.Add(lesson);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { courseId = vm.CourseId });
    }

    // GET: /ManagerLessons/Edit/{id}
    public async Task<IActionResult> Edit(Guid id)
    {
        var lesson = await _db.Lessons.FirstOrDefaultAsync(x => x.Id == id);
        if (lesson == null) return NotFound();

        var courseTitle = await _db.Courses
            .Where(x => x.Id == lesson.CourseId)
            .Select(x => x.Title)
            .FirstOrDefaultAsync() ?? "";

        var vm = new LessonEditVm
        {
            Id = lesson.Id,
            CourseId = lesson.CourseId,
            CourseTitle = courseTitle,
            Order = lesson.Order,
            Title = lesson.Title,
            Description = lesson.Description,
            Content = lesson.Content,
            IsPublished = lesson.IsPublished,
            PriceAmount = lesson.PriceAmount,
            Currency = lesson.Currency
        };

        return View(vm);
    }

    // POST: /ManagerLessons/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(LessonEditVm vm)
    {
        if (!ModelState.IsValid)
        {
            vm.CourseTitle = await _db.Courses.Where(x => x.Id == vm.CourseId).Select(x => x.Title).FirstOrDefaultAsync() ?? string.Empty;
            return View(vm);
        }

        var lesson = await _db.Lessons.FirstOrDefaultAsync(x => x.Id == vm.Id);
        if (lesson == null) return NotFound();

        // унікальність Order в курсі (крім себе)
        var orderExists = await _db.Lessons.AnyAsync(x =>
            x.CourseId == vm.CourseId &&
            x.Order == vm.Order &&
            x.Id != vm.Id);

        if (orderExists)
        {
            ModelState.AddModelError(nameof(vm.Order), "Такий Order вже існує в цьому курсі.");
            vm.CourseTitle = await _db.Courses.Where(x => x.Id == vm.CourseId).Select(x => x.Title).FirstOrDefaultAsync() ?? string.Empty;
            return View(vm);
        }

        lesson.Order = vm.Order;
        lesson.Title = vm.Title.Trim();
        lesson.Description = vm.Description?.Trim();
        lesson.Content = vm.Content;
        lesson.IsPublished = vm.IsPublished;
        lesson.PriceAmount = vm.PriceAmount;
        lesson.Currency = string.IsNullOrWhiteSpace(vm.Currency) ? "UAH" : vm.Currency.Trim().ToUpperInvariant();

        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { courseId = vm.CourseId });
    }

    // GET: /ManagerLessons/Delete/{id}
    public async Task<IActionResult> Delete(Guid id)
    {
        var lesson = await _db.Lessons.FirstOrDefaultAsync(x => x.Id == id);
        if (lesson == null) return NotFound();

        var courseTitle = await _db.Courses
            .Where(x => x.Id == lesson.CourseId)
            .Select(x => x.Title)
            .FirstOrDefaultAsync() ?? "";

        var vm = new LessonEditVm
        {
            Id = lesson.Id,
            CourseId = lesson.CourseId,
            CourseTitle = courseTitle,
            Order = lesson.Order,
            Title = lesson.Title,
            Description = lesson.Description,
            IsPublished = lesson.IsPublished,
            PriceAmount = lesson.PriceAmount,
            Currency = lesson.Currency
        };

        return View(vm);
    }

    // POST: /ManagerLessons/DeleteConfirmed
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id, Guid courseId)
    {
        var lesson = await _db.Lessons.FirstOrDefaultAsync(x => x.Id == id);
        if (lesson == null) return NotFound();

        _db.Lessons.Remove(lesson);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { courseId });
    }
}
