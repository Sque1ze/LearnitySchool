using LearnitySchool.Application.Common;
using LearnitySchool.Domain.Entities;
using LearnitySchool.Domain.Enums;
using LearnitySchool.Infrastructure.Identity;
using LearnitySchool.Infrastructure.Persistence;
using LearnitySchool.Web.ViewModels.Teacher.Submissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LearnitySchool.Web.Controllers;

[Authorize(Roles = RoleNames.Teacher)]
public class TeacherSubmissionsController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public TeacherSubmissionsController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    private static string Display(ApplicationUser u)
    {
        var first = (u.FirstName ?? string.Empty).Trim();
        var last = (u.LastName ?? string.Empty).Trim();
        if (!string.IsNullOrWhiteSpace(first) && !string.IsNullOrWhiteSpace(last)) return $"{first} {last}";
        if (!string.IsNullOrWhiteSpace(first)) return first;
        if (!string.IsNullOrWhiteSpace(last)) return last;
        return u.Email ?? u.UserName ?? u.Id;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] TeacherSubmissionListVm vm)
    {
        var teacherId = _userManager.GetUserId(User);

        vm.Courses = await _db.CourseTeachers
            .Where(t => t.TeacherUserId == teacherId)
            .Select(t => new SelectListItem
            {
                Value = t.CourseId.ToString(),
                Text = t.Course.Title,
                Selected = vm.CourseId.HasValue && vm.CourseId.Value == t.CourseId
            })
            .OrderBy(x => x.Text)
            .ToListAsync();

        var q = _db.StudentTaskSubmissions
            .AsNoTracking()
            .Where(s => s.LessonTask.Lesson.Course.Teachers.Any(t => t.TeacherUserId == teacherId));

        if (vm.Status.HasValue)
        {
            q = q.Where(s => s.Status == vm.Status.Value);
        }

        if (vm.CourseId.HasValue)
        {
            q = q.Where(s => s.LessonTask.Lesson.CourseId == vm.CourseId.Value);
        }

        if (vm.From.HasValue)
        {
            var from = vm.From.Value.Date;
            q = q.Where(s => s.SubmittedAtUtc >= from);
        }

        if (vm.To.HasValue)
        {
            var toExclusive = vm.To.Value.Date.AddDays(1);
            q = q.Where(s => s.SubmittedAtUtc < toExclusive);
        }

        if (!string.IsNullOrWhiteSpace(vm.Q))
        {
            var term = vm.Q.Trim().ToLower();
            q = q.Where(s =>
                s.LessonTask.Title.ToLower().Contains(term) ||
                s.LessonTask.Lesson.Title.ToLower().Contains(term) ||
                s.LessonTask.Lesson.Course.Title.ToLower().Contains(term) ||
                _db.Users.Any(u => u.Id == s.StudentUserId &&
                    (((u.FirstName ?? "") + " " + (u.LastName ?? "")).ToLower().Contains(term) ||
                     (u.Email ?? "").ToLower().Contains(term))));
        }

        var rows = await q
            .OrderBy(s => s.Status)
            .ThenByDescending(s => s.SubmittedAtUtc)
            .Select(s => new
            {
                SubmissionId = s.Id,
                CourseId = s.LessonTask.Lesson.CourseId,
                LessonId = s.LessonTask.LessonId,
                TaskId = s.LessonTaskId,
                s.StudentUserId,
                CourseTitle = s.LessonTask.Lesson.Course.Title,
                LessonTitle = s.LessonTask.Lesson.Title,
                TaskTitle = s.LessonTask.Title,
                s.Status,
                s.ServerSimilarity,
                s.SubmittedAtUtc
            })
            .ToListAsync();

        var studentIds = rows.Select(x => x.StudentUserId).Distinct().ToList();
        var peopleList = await _db.Users.AsNoTracking()
            .Where(u => studentIds.Contains(u.Id))
            .ToListAsync();

        var people = peopleList.ToDictionary(u => u.Id, u => new { Name = Display(u), u.Email });

        vm.Items = rows.Select(x =>
        {
            people.TryGetValue(x.StudentUserId, out var p);
            return new TeacherSubmissionRowVm
            {
                SubmissionId = x.SubmissionId,
                CourseId = x.CourseId,
                LessonId = x.LessonId,
                TaskId = x.TaskId,
                StudentName = p?.Name ?? "—",
                StudentEmail = p?.Email,
                CourseTitle = x.CourseTitle,
                LessonTitle = x.LessonTitle,
                TaskTitle = x.TaskTitle,
                Status = x.Status,
                ServerSimilarity = x.ServerSimilarity,
                SubmittedAtUtc = x.SubmittedAtUtc
            };
        }).ToList();

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var teacherId = _userManager.GetUserId(User);

        var row = await _db.StudentTaskSubmissions
            .AsNoTracking()
            .Where(s => s.Id == id && s.LessonTask.Lesson.Course.Teachers.Any(t => t.TeacherUserId == teacherId))
            .Select(s => new
            {
                SubmissionId = s.Id,
                CourseId = s.LessonTask.Lesson.CourseId,
                LessonId = s.LessonTask.LessonId,
                TaskId = s.LessonTaskId,
                s.StudentUserId,
                CourseTitle = s.LessonTask.Lesson.Course.Title,
                LessonTitle = s.LessonTask.Lesson.Title,
                TaskTitle = s.LessonTask.Title,
                TaskDescription = s.LessonTask.Description,
                s.Status,
                s.ClientSimilarity,
                s.ServerSimilarity,
                s.SubmittedAtUtc,
                s.ReviewedAtUtc,
                s.TeacherComment,
                s.Html,
                s.Css,
                s.Js
            })
            .FirstOrDefaultAsync();

        if (row == null) return NotFound();

        var student = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == row.StudentUserId);

        var practice = await _db.PracticeTasks
            .AsNoTracking()
            .Where(p => p.LessonTaskId == row.TaskId)
            .Select(p => new
            {
                p.Statement,
                p.ReferenceHtml,
                p.ReferenceCss,
                p.ReferenceJs
            })
            .FirstOrDefaultAsync();

        var vm = new TeacherSubmissionDetailsVm
        {
            SubmissionId = row.SubmissionId,
            CourseId = row.CourseId,
            LessonId = row.LessonId,
            TaskId = row.TaskId,
            StudentUserId = row.StudentUserId,
            StudentName = student == null ? "—" : Display(student),
            StudentEmail = student?.Email,
            CourseTitle = row.CourseTitle,
            LessonTitle = row.LessonTitle,
            TaskTitle = row.TaskTitle,
            TaskDescription = row.TaskDescription,
            Statement = practice?.Statement,
            ReferenceHtml = practice?.ReferenceHtml ?? string.Empty,
            ReferenceCss = practice?.ReferenceCss ?? string.Empty,
            ReferenceJs = practice?.ReferenceJs ?? string.Empty,
            Status = row.Status,
            ClientSimilarity = row.ClientSimilarity,
            ServerSimilarity = row.ServerSimilarity,
            SubmittedAtUtc = row.SubmittedAtUtc,
            ReviewedAtUtc = row.ReviewedAtUtc,
            TeacherComment = row.TeacherComment,
            Html = row.Html,
            Css = row.Css,
            Js = row.Js
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(Guid id, string decision, string? comment)
    {
        var teacherId = _userManager.GetUserId(User);

        var submission = await _db.StudentTaskSubmissions
            .FirstOrDefaultAsync(s =>
                s.Id == id &&
                s.LessonTask.Lesson.Course.Teachers.Any(t => t.TeacherUserId == teacherId));

        if (submission == null) return NotFound();

        var approve = string.Equals(decision, "approve", StringComparison.OrdinalIgnoreCase);
        var returnWork = string.Equals(decision, "return", StringComparison.OrdinalIgnoreCase);

        if (!approve && !returnWork) return BadRequest("Unknown review decision.");

        submission.Status = approve ? StudentSubmissionStatus.Approved : StudentSubmissionStatus.Returned;
        submission.ReviewedAtUtc = DateTime.UtcNow;
        submission.ReviewedByTeacherUserId = teacherId;
        submission.TeacherComment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();

        await SetTaskProgressAsync(submission.LessonTaskId, submission.StudentUserId, approve);

        _db.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = submission.StudentUserId,
            Title = approve ? "Роботу зараховано" : "Роботу повернуто",
            Message = approve ? "Вчитель зарахував вашу роботу ✅" : "Вчитель повернув роботу на доопрацювання 🔁",
            Type = NotificationType.Review,
            Url = Url.Action("Practice", "Student", new { taskId = submission.LessonTaskId })
        });

        await _db.SaveChangesAsync();

        TempData["Success"] = approve
            ? "Роботу зараховано ✅"
            : "Роботу повернуто учню на доопрацювання 🔁";

        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task SetTaskProgressAsync(Guid taskId, string studentUserId, bool isCompleted)
    {
        var progress = await _db.StudentTaskProgresses
            .FirstOrDefaultAsync(p => p.TaskId == taskId && p.StudentUserId == studentUserId);

        if (progress == null)
        {
            progress = new StudentTaskProgress
            {
                Id = Guid.NewGuid(),
                TaskId = taskId,
                StudentUserId = studentUserId
            };
            _db.StudentTaskProgresses.Add(progress);
        }

        progress.IsCompleted = isCompleted;
        progress.CompletedAt = isCompleted ? DateTime.UtcNow : null;
    }
}
