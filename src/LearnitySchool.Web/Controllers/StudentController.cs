using LearnitySchool.Domain.Entities;
using LearnitySchool.Infrastructure.Identity;
using LearnitySchool.Infrastructure.Persistence;
using LearnitySchool.Web.Common;
using LearnitySchool.Web.ViewModels.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearnitySchool.Web.Controllers;

[Authorize(Roles = RoleNames.Student)]
public class StudentController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public StudentController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // =========================
    // MY COURSES + PROGRESS
    // =========================
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);

        var courses = await _db.CourseStudents
            .Where(cs => cs.StudentUserId == userId)
            .Select(cs => cs.Course)
            .Where(c => c.IsPublished)
            .OrderBy(c => c.Title)
            .Select(c => new StudentCourseVm
            {
                CourseId = c.Id,
                Title = c.Title,
                Description = c.Description,

                LessonsCount = c.Lessons.Count(l => l.IsPublished),

                TotalTasks = c.Lessons
                    .Where(l => l.IsPublished)
                    .SelectMany(l => l.Tasks)
                    .Count(t => t.IsPublished),

                CompletedTasks = _db.StudentTaskProgresses.Count(p =>
                    p.StudentUserId == userId &&
                    p.IsCompleted &&
                    p.Task.Lesson.CourseId == c.Id),

                ProgressPercent = 0
            })
            .ToListAsync();

        foreach (var c in courses)
        {
            c.ProgressPercent = c.TotalTasks == 0
                ? 0
                : (int)Math.Round((double)c.CompletedTasks * 100 / c.TotalTasks);
        }

        return View(courses);
    }

    // =========================
    // LESSONS OF COURSE + PROGRESS
    // =========================
    public async Task<IActionResult> Lessons(Guid courseId)
    {
        var userId = _userManager.GetUserId(User);

        var lessons = await _db.Lessons
            .Where(l => l.CourseId == courseId && l.IsPublished)
            .OrderBy(l => l.Order)
            .Select(l => new StudentLessonVm
            {
                LessonId = l.Id,
                Order = l.Order,
                Title = l.Title,

                TasksCount = _db.LessonTasks.Count(t => t.LessonId == l.Id && t.IsPublished),

                CompletedTasks = _db.StudentTaskProgresses.Count(p =>
                    p.StudentUserId == userId &&
                    p.IsCompleted &&
                    p.Task.LessonId == l.Id),

                ProgressPercent = 0
            })
            .ToListAsync();

        foreach (var l in lessons)
        {
            l.ProgressPercent = l.TasksCount == 0
                ? 0
                : (int)Math.Round((double)l.CompletedTasks * 100 / l.TasksCount);
        }

        ViewBag.CourseId = courseId;
        return View(lessons);
    }

    // =========================
    // TASKS OF LESSON
    // =========================
    public async Task<IActionResult> Tasks(Guid lessonId)
    {
        var userId = _userManager.GetUserId(User);

        var courseId = await _db.Lessons
            .Where(l => l.Id == lessonId)
            .Select(l => l.CourseId)
            .FirstOrDefaultAsync();

        var tasks = await _db.LessonTasks
            .Where(t => t.LessonId == lessonId && t.IsPublished)
            .OrderBy(t => t.Order)
            .Select(t => new StudentTaskVm
            {
                TaskId = t.Id,
                Order = t.Order,
                Title = t.Title,
                Type = t.Type,

                IsCompleted = _db.StudentTaskProgresses.Any(p =>
                    p.TaskId == t.Id &&
                    p.StudentUserId == userId &&
                    p.IsCompleted)
            })
            .ToListAsync();

        ViewBag.LessonId = lessonId;
        ViewBag.CourseId = courseId;

        return View(tasks);
    }

    // =========================
    // COMPLETE TASK (POST)
    // =========================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteTask(Guid taskId, Guid lessonId)
    {
        var userId = _userManager.GetUserId(User);

        var task = await _db.LessonTasks
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == taskId && t.LessonId == lessonId && t.IsPublished);

        if (task == null) return NotFound();

        var courseId = await _db.Lessons
            .Where(l => l.Id == lessonId)
            .Select(l => l.CourseId)
            .FirstOrDefaultAsync();

        var allowed = await _db.CourseStudents.AnyAsync(cs =>
            cs.CourseId == courseId && cs.StudentUserId == userId);

        if (!allowed) return Forbid();

        var progress = await _db.StudentTaskProgresses
            .FirstOrDefaultAsync(p => p.TaskId == taskId && p.StudentUserId == userId);

        if (progress == null)
        {
            progress = new StudentTaskProgress
            {
                Id = Guid.NewGuid(),
                TaskId = taskId,
                StudentUserId = userId!,
                IsCompleted = true,
                CompletedAt = DateTime.UtcNow
            };

            _db.StudentTaskProgresses.Add(progress);
        }
        else
        {
            progress.IsCompleted = true;
            progress.CompletedAt = DateTime.UtcNow;
            _db.StudentTaskProgresses.Update(progress);
        }

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Tasks), new { lessonId });
    }

    public async Task<IActionResult> TaskDetails(Guid taskId)
    {
        var userId = _userManager.GetUserId(User);

        // 1) Витягаємо задачу + lessonId + courseId
        var task = await _db.LessonTasks
            .Where(t => t.Id == taskId && t.IsPublished)
            .Select(t => new
            {
                t.Id,
                t.LessonId,
                t.Order,
                t.Title,
                t.Description,
                Type = t.Type, // enum або string
                CourseId = t.Lesson.CourseId
            })
            .FirstOrDefaultAsync();

        if (task == null)
            return NotFound();

        // 2) Перевірка доступу: студент має бути записаний на курс
        var allowed = await _db.CourseStudents.AnyAsync(cs =>
            cs.CourseId == task.CourseId && cs.StudentUserId == userId);

        if (!allowed)
            return Forbid();

        // 3) Чи вже виконано
        var isCompleted = await _db.StudentTaskProgresses.AnyAsync(p =>
            p.TaskId == task.Id && p.StudentUserId == userId && p.IsCompleted);

        var vm = new StudentTaskDetailsVm
        {
            TaskId = task.Id,
            LessonId = task.LessonId,
            CourseId = task.CourseId,
            Order = task.Order,
            Title = task.Title,
            Description = task.Description ?? "",
            Type = task.Type.ToString(),
            IsCompleted = isCompleted
        };

        return View(vm);
    }


    // =========================
    // PROFILE
    // =========================
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var vm = new StudentProfileVm
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? "",
            UserName = user.UserName ?? ""
        };

        return View(vm);
    }
}
