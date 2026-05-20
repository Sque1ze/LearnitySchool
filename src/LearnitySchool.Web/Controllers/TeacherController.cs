using LearnitySchool.Application.Common;
using LearnitySchool.Infrastructure.Identity;
using LearnitySchool.Infrastructure.Persistence;
using LearnitySchool.Web.ViewModels.Teacher;
using LearnitySchool.Web.ViewModels.Teacher.Students;
using LearnitySchool.Web.ViewModels.Teacher.Dashboard;
using LearnitySchool.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearnitySchool.Web.Controllers;

[Authorize(Roles = RoleNames.Teacher)]
public class TeacherController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public TeacherController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    private static string Display(ApplicationUser u)
    {
        var first = (u.FirstName ?? "").Trim();
        var last = (u.LastName ?? "").Trim();
        if (!string.IsNullOrWhiteSpace(first) && !string.IsNullOrWhiteSpace(last))
            return $"{first} {last}";
        if (!string.IsNullOrWhiteSpace(first)) return first;
        if (!string.IsNullOrWhiteSpace(last)) return last;
        return u.Email ?? u.UserName ?? u.Id;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] TeacherStaffVm vm)
    {
        var teacherUsers = await _userManager.GetUsersInRoleAsync(RoleNames.Teacher);
        var managerUsers = await _userManager.GetUsersInRoleAsync(RoleNames.Manager);

        var allowedIds = teacherUsers
            .Select(u => u.Id)
            .Concat(managerUsers.Select(u => u.Id))
            .Distinct()
            .ToHashSet();

        var usersQ = _userManager.Users
            .AsNoTracking()
            .Where(u => allowedIds.Contains(u.Id))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(vm.Q))
        {
            var term = vm.Q.Trim().ToLower();
            usersQ = usersQ.Where(u =>
                ((u.FirstName ?? "") + " " + (u.LastName ?? "")).ToLower().Contains(term) ||
                ((u.LastName ?? "") + " " + (u.FirstName ?? "")).ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(vm.Login))
        {
            var login = vm.Login.Trim().ToLower();
            usersQ = usersQ.Where(u => (u.UserName ?? "").ToLower().Contains(login));
        }

        if (!string.IsNullOrWhiteSpace(vm.Phone))
        {
            var phone = vm.Phone.Trim();
            usersQ = usersQ.Where(u => (u.PhoneNumber ?? "").Contains(phone));
        }

        if (!string.IsNullOrWhiteSpace(vm.Email))
        {
            var email = vm.Email.Trim().ToLower();
            usersQ = usersQ.Where(u => (u.Email ?? "").ToLower().Contains(email));
        }

        if (!string.IsNullOrWhiteSpace(vm.Role))
        {
            var role = vm.Role.Trim();

            if (role != RoleNames.Teacher && role != RoleNames.Manager)
            {
                vm.Items = new();
                return View(vm);
            }

            var roleUsers = await _userManager.GetUsersInRoleAsync(role);
            var roleIds = roleUsers.Select(x => x.Id).ToHashSet();

            usersQ = usersQ.Where(u => roleIds.Contains(u.Id));
        }

        var users = await usersQ
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();

        vm.Items = new List<TeacherStaffVm.StaffRowVm>();

        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            var role = roles.Contains(RoleNames.Manager) ? RoleNames.Manager : RoleNames.Teacher;

            vm.Items.Add(new TeacherStaffVm.StaffRowVm
            {
                UserId = u.Id,
                FullName = Display(u),
                Email = u.Email,
                Phone = u.PhoneNumber,
                Login = u.UserName,
                Role = role
            });
        }

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return RedirectToAction("Login", "Account");

        var vm = new TeacherProfileVm
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? "",
            UserName = user.UserName ?? ""
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Students([FromQuery] TeacherStudentsVm vm)
    {
        var teacherId = _userManager.GetUserId(User);

        var courseIds = await _db.CourseTeachers
            .AsNoTracking()
            .Where(x => x.TeacherUserId == teacherId)
            .Select(x => x.CourseId)
            .Distinct()
            .ToListAsync();

        if (courseIds.Count == 0)
        {
            vm.Items = new();
            return View(vm);
        }

        var studentIds = await _db.CourseStudents
            .AsNoTracking()
            .Where(cs => courseIds.Contains(cs.CourseId))
            .Select(cs => cs.StudentUserId)
            .Distinct()
            .ToListAsync();

        if (studentIds.Count == 0)
        {
            vm.Items = new();
            return View(vm);
        }

        var q = _db.Users
            .AsNoTracking()
            .Where(u => studentIds.Contains(u.Id))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(vm.Q))
        {
            var term = vm.Q.Trim().ToLower();
            q = q.Where(u =>
                ((u.FirstName ?? "") + " " + (u.LastName ?? "")).ToLower().Contains(term) ||
                ((u.LastName ?? "") + " " + (u.FirstName ?? "")).ToLower().Contains(term) ||
                (u.Email ?? "").ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(vm.Login))
        {
            var login = vm.Login.Trim().ToLower();
            q = q.Where(u => (u.UserName ?? "").ToLower().Contains(login));
        }

        if (!string.IsNullOrWhiteSpace(vm.Phone))
        {
            var phone = vm.Phone.Trim();
            q = q.Where(u => (u.PhoneNumber ?? "").Contains(phone));
        }

        if (vm.AgeFrom.HasValue)
        {
            var min = vm.AgeFrom.Value;
            q = q.Where(u => u.Age != null && u.Age >= min);
        }

        if (vm.AgeTo.HasValue)
        {
            var max = vm.AgeTo.Value;
            q = q.Where(u => u.Age != null && u.Age <= max);
        }

        var groupCounts = await _db.CourseStudents
            .AsNoTracking()
            .Where(cs => courseIds.Contains(cs.CourseId) && studentIds.Contains(cs.StudentUserId))
            .GroupBy(cs => cs.StudentUserId)
            .Select(g => new
            {
                StudentUserId = g.Key,
                Cnt = g.Select(x => x.CourseId).Distinct().Count()
            })
            .ToDictionaryAsync(x => x.StudentUserId, x => x.Cnt);

        var users = await q
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();

        vm.Items = users.Select(u => new TeacherStudentsVm.TeacherStudentRowVm
        {
            StudentUserId = u.Id,
            FullName = Display(u),
            Email = u.Email,
            Phone = u.PhoneNumber,
            Age = u.Age,
            Login = u.UserName,
            GroupsCount = groupCounts.TryGetValue(u.Id, out var cnt) ? cnt : 0
        }).ToList();

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Schedule([FromQuery] TeacherScheduleVm vm)
    {
        var teacherId = _userManager.GetUserId(User);

        var courses = await _db.Courses
            .AsNoTracking()
            .Where(c => c.Teachers.Any(t => t.TeacherUserId == teacherId))
            .Select(c => new
            {
                c.Id,
                c.Title,
                StudentsCount = c.Students.Count()
            })
            .ToListAsync();

        if (courses.Count == 0)
            return View(vm);

        var courseIds = courses.Select(c => c.Id).ToList();

        var lessons = await _db.Lessons
            .AsNoTracking()
            .Where(l => courseIds.Contains(l.CourseId) && l.IsPublished)
            .Select(l => new
            {
                l.Id,
                l.CourseId,
                l.Order,
                l.Title
            })
            .ToListAsync();

        var accessRows = await _db.LessonAccesses
            .AsNoTracking()
            .Where(a => courseIds.Contains(a.CourseId))
            .Select(a => new { a.CourseId, a.LessonId, a.IsOpen })
            .ToListAsync();

        var accessMap = accessRows.ToDictionary(x => (x.CourseId, x.LessonId), x => x.IsOpen);

        var lessonsByCourse = lessons
            .GroupBy(l => l.CourseId)
            .ToDictionary(g => g.Key, g => g.OrderBy(x => x.Order).ToList());

        var rows = new List<TeacherScheduleVm.RowVm>();

        foreach (var c in courses)
        {
            if (!lessonsByCourse.TryGetValue(c.Id, out var list) || list.Count == 0)
                continue;

            var next = list.FirstOrDefault(l =>
            {
                var isOpen = accessMap.TryGetValue((c.Id, l.Id), out var open) && open;
                return !isOpen; 
            });

            if (next == null)
                continue; 

            rows.Add(new TeacherScheduleVm.RowVm
            {
                CourseId = c.Id,
                CourseTitle = c.Title,
                StudentsCount = c.StudentsCount,

                LessonId = next.Id,
                LessonOrder = next.Order,
                LessonTitle = next.Title,

                IsOpen = false
            });
        }

        if (!string.IsNullOrWhiteSpace(vm.GroupQ))
        {
            var term = vm.GroupQ.Trim().ToLower();
            rows = rows.Where(r => (r.CourseTitle ?? "").ToLower().Contains(term)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(vm.LessonQ))
        {
            var term = vm.LessonQ.Trim().ToLower();
            rows = rows.Where(r => (r.LessonTitle ?? "").ToLower().Contains(term)).ToList();
        }

        if (vm.StudentsCount.HasValue)
        {
            var n = vm.StudentsCount.Value;
            rows = rows.Where(r => r.StudentsCount == n).ToList();
        }

        vm.Items = rows
            .OrderBy(r => r.CourseTitle)
            .ThenBy(r => r.LessonOrder)
            .ToList();

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var teacherId = _userManager.GetUserId(User);

        var courseIds = await _db.CourseTeachers
            .Where(t => t.TeacherUserId == teacherId)
            .Select(t => t.CourseId)
            .ToListAsync();

        var vm = new TeacherDashboardVm
        {
            PendingSubmissions = await _db.StudentTaskSubmissions.CountAsync(s => courseIds.Contains(s.LessonTask.Lesson.CourseId) && s.Status == StudentSubmissionStatus.PendingReview),
            ApprovedSubmissions = await _db.StudentTaskSubmissions.CountAsync(s => courseIds.Contains(s.LessonTask.Lesson.CourseId) && s.Status == StudentSubmissionStatus.Approved),
            ReturnedSubmissions = await _db.StudentTaskSubmissions.CountAsync(s => courseIds.Contains(s.LessonTask.Lesson.CourseId) && s.Status == StudentSubmissionStatus.Returned),
            PaidLessonsCount = await _db.LessonPayments.CountAsync(p => courseIds.Contains(p.Lesson.CourseId) && p.Status == PaymentStatus.Paid)
        };

        var paidRequired = await _db.CourseStudents
            .Where(cs => courseIds.Contains(cs.CourseId))
            .SelectMany(cs => cs.Course.Lessons.Where(l => l.IsPublished && l.PriceAmount > 0).Select(l => new { cs.StudentUserId, LessonId = l.Id }))
            .ToListAsync();

        var paidPairs = await _db.LessonPayments
            .Where(p => p.Status == PaymentStatus.Paid && courseIds.Contains(p.Lesson.CourseId))
            .Select(p => new { p.StudentId, p.LessonId })
            .ToListAsync();

        var paidSet = paidPairs.Select(p => $"{p.StudentId}:{p.LessonId}").ToHashSet();
        vm.UnpaidPaidRequiredCount = paidRequired.Count(x => !paidSet.Contains($"{x.StudentUserId}:{x.LessonId}"));

        var recent = await _db.StudentTaskSubmissions
            .AsNoTracking()
            .Where(s => courseIds.Contains(s.LessonTask.Lesson.CourseId))
            .OrderByDescending(s => s.SubmittedAtUtc)
            .Take(6)
            .Select(s => new
            {
                s.Id,
                s.StudentUserId,
                CourseTitle = s.LessonTask.Lesson.Course.Title,
                LessonTitle = s.LessonTask.Lesson.Title,
                TaskTitle = s.LessonTask.Title,
                s.SubmittedAtUtc
            })
            .ToListAsync();

        var studentIds = recent.Select(x => x.StudentUserId).Distinct().ToList();
        var peopleList = await _db.Users.AsNoTracking()
            .Where(u => studentIds.Contains(u.Id))
            .ToListAsync();
        var people = peopleList.ToDictionary(u => u.Id, Display);

        vm.RecentSubmissions = recent.Select(x => new TeacherDashboardSubmissionRowVm
        {
            SubmissionId = x.Id,
            StudentName = people.TryGetValue(x.StudentUserId, out var name) ? name : "—",
            CourseTitle = x.CourseTitle,
            LessonTitle = x.LessonTitle,
            TaskTitle = x.TaskTitle,
            SubmittedAtUtc = x.SubmittedAtUtc
        }).ToList();

        vm.Courses = await _db.Courses
            .AsNoTracking()
            .Where(c => courseIds.Contains(c.Id))
            .OrderBy(c => c.Title)
            .Select(c => new TeacherDashboardCourseRowVm
            {
                CourseId = c.Id,
                Title = c.Title,
                StudentsCount = c.Students.Count(),
                LessonsCount = c.Lessons.Count()
            })
            .ToListAsync();

        return View(vm);
    }

}