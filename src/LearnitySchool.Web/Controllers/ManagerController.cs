using LearnitySchool.Infrastructure.Identity;
using LearnitySchool.Application.Common;
using LearnitySchool.Domain.Enums;
using LearnitySchool.Infrastructure.Persistence;
using LearnitySchool.Web.ViewModels.Manager;
using LearnitySchool.Web.ViewModels.Manager.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearnitySchool.Web.Controllers;

[Authorize(Roles = RoleNames.Manager)]
public class ManagerController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _db;

    public ManagerController(UserManager<ApplicationUser> userManager, AppDbContext db)
    {
        _userManager = userManager;
        _db = db;
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

    public async Task<IActionResult> Index()
    {
        var managerId = _userManager.GetUserId(User);

        var students = await _userManager.GetUsersInRoleAsync(RoleNames.Student);
        var teachers = await _userManager.GetUsersInRoleAsync(RoleNames.Teacher);
        var managers = await _userManager.GetUsersInRoleAsync(RoleNames.Manager);

        var pendingRows = await _db.StudentTaskSubmissions
            .AsNoTracking()
            .Where(s => s.Status == StudentSubmissionStatus.PendingReview)
            .OrderByDescending(s => s.SubmittedAtUtc)
            .Take(6)
            .Select(s => new
            {
                s.Id,
                s.StudentUserId,
                CourseTitle = s.LessonTask.Lesson.Course.Title,
                LessonTitle = s.LessonTask.Lesson.Title,
                TaskTitle = s.LessonTask.Title,
                s.Status,
                s.SubmittedAtUtc
            })
            .ToListAsync();

        var studentIds = pendingRows.Select(x => x.StudentUserId).Distinct().ToList();
        var peopleList = await _db.Users
            .AsNoTracking()
            .Where(u => studentIds.Contains(u.Id))
            .ToListAsync();
        var people = peopleList.ToDictionary(u => u.Id, Display);

        var vm = new ManagerDashboardVm
        {
            StudentsCount = students.Count,
            TeachersCount = teachers.Count,
            ManagersCount = managers.Count,
            CoursesCount = await _db.Courses.CountAsync(),
            PublishedCoursesCount = await _db.Courses.CountAsync(c => c.IsPublished),
            LessonsCount = await _db.Lessons.CountAsync(),
            TasksCount = await _db.LessonTasks.CountAsync(),
            PendingSubmissionsCount = await _db.StudentTaskSubmissions.CountAsync(s => s.Status == StudentSubmissionStatus.PendingReview),
            PendingPaymentsCount = await _db.LessonPayments.CountAsync(p => p.Status == PaymentStatus.Pending),
            UnreadNotificationsCount = string.IsNullOrWhiteSpace(managerId) ? 0 : await _db.Notifications.CountAsync(n => n.UserId == managerId && !n.IsRead),
            RecentCourses = await _db.Courses
                .AsNoTracking()
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
                .Select(c => new ManagerCourseDashboardRowVm
                {
                    CourseId = c.Id,
                    Title = c.Title,
                    IsPublished = c.IsPublished,
                    LessonsCount = c.Lessons.Count,
                    StudentsCount = c.Students.Count,
                    TeachersCount = c.Teachers.Count,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync(),
            PendingSubmissions = pendingRows.Select(x => new ManagerSubmissionDashboardRowVm
            {
                SubmissionId = x.Id,
                StudentName = people.TryGetValue(x.StudentUserId, out var name) ? name : "—",
                CourseTitle = x.CourseTitle,
                LessonTitle = x.LessonTitle,
                TaskTitle = x.TaskTitle,
                Status = x.Status,
                SubmittedAtUtc = x.SubmittedAtUtc
            }).ToList()
        };

        return View(vm);
    }

    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return RedirectToAction("Login", "Account");

        var vm = new ManagerProfileVm
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? "",
            UserName = user.UserName ?? ""
        };

        return View(vm);
    }
}
