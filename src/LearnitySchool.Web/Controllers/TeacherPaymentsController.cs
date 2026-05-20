using LearnitySchool.Application.Common;
using LearnitySchool.Domain.Enums;
using LearnitySchool.Infrastructure.Identity;
using LearnitySchool.Infrastructure.Persistence;
using LearnitySchool.Web.ViewModels.Teacher.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LearnitySchool.Web.Controllers;

[Authorize(Roles = RoleNames.Teacher)]
public class TeacherPaymentsController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public TeacherPaymentsController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    private static string Display(ApplicationUser u)
    {
        var first = (u.FirstName ?? "").Trim();
        var last = (u.LastName ?? "").Trim();
        return !string.IsNullOrWhiteSpace(first + last) ? $"{first} {last}".Trim() : (u.Email ?? u.UserName ?? u.Id);
    }

    public async Task<IActionResult> Index(Guid? courseId)
    {
        var teacherId = _userManager.GetUserId(User);
        var courseIds = await _db.CourseTeachers.Where(t => t.TeacherUserId == teacherId).Select(t => t.CourseId).ToListAsync();
        if (courseId.HasValue && !courseIds.Contains(courseId.Value)) return Forbid();

        var vm = new TeacherPaymentsVm { CourseId = courseId };
        vm.Courses = await _db.Courses.Where(c => courseIds.Contains(c.Id)).OrderBy(c => c.Title)
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Title, Selected = courseId.HasValue && courseId == c.Id }).ToListAsync();

        var baseQuery = _db.CourseStudents.Where(cs => courseIds.Contains(cs.CourseId));
        if (courseId.HasValue) baseQuery = baseQuery.Where(cs => cs.CourseId == courseId.Value);

        var rows = await baseQuery
            .SelectMany(cs => cs.Course.Lessons.Where(l => l.IsPublished && l.PriceAmount > 0).Select(l => new
            {
                cs.StudentUserId,
                CourseTitle = cs.Course.Title,
                LessonId = l.Id,
                LessonOrder = l.Order,
                LessonTitle = l.Title,
                l.PriceAmount,
                l.Currency
            }))
            .OrderBy(x => x.CourseTitle).ThenBy(x => x.LessonOrder)
            .ToListAsync();

        var studentIds = rows.Select(r => r.StudentUserId).Distinct().ToList();
        var studentList = await _db.Users.AsNoTracking().Where(u => studentIds.Contains(u.Id)).ToListAsync();
        var students = studentList.ToDictionary(u => u.Id, u => new { Name = Display(u), u.Email });
        var lessonIds = rows.Select(r => r.LessonId).Distinct().ToList();
        var paid = await _db.LessonPayments.AsNoTracking().Where(p => p.Status == PaymentStatus.Paid && lessonIds.Contains(p.LessonId)).Select(p => new { p.StudentId, p.LessonId, p.PaidAtUtc }).ToListAsync();
        var paidMap = paid.GroupBy(p => $"{p.StudentId}:{p.LessonId}").ToDictionary(g => g.Key, g => g.Max(x => x.PaidAtUtc));

        vm.Items = rows.Select(r =>
        {
            students.TryGetValue(r.StudentUserId, out var s);
            var key = $"{r.StudentUserId}:{r.LessonId}";
            return new TeacherPaymentRowVm
            {
                StudentName = s?.Name ?? "—",
                StudentEmail = s?.Email,
                CourseTitle = r.CourseTitle,
                LessonOrder = r.LessonOrder,
                LessonTitle = r.LessonTitle,
                PriceAmount = r.PriceAmount,
                Currency = r.Currency,
                IsPaid = paidMap.ContainsKey(key),
                PaidAtUtc = paidMap.TryGetValue(key, out var paidAt) ? paidAt : null
            };
        }).ToList();

        return View(vm);
    }
}
