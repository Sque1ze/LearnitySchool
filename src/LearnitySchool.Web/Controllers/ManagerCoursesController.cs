using LearnitySchool.Domain.Entities;
using LearnitySchool.Infrastructure.Identity;
using LearnitySchool.Infrastructure.Persistence;
using LearnitySchool.Application.Common;
using LearnitySchool.Web.ViewModels.Manager.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LearnitySchool.Web.Controllers;

[Authorize(Roles = RoleNames.Manager)]
public class ManagerCoursesController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public ManagerCoursesController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // GET: /ManagerCourses
    public async Task<IActionResult> Index()
    {
        var courses = await _db.Courses
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new CourseEditVm
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                IsPublished = x.IsPublished
            })
            .ToListAsync();

        return View(courses);
    }

    // GET: /ManagerCourses/Create
    public IActionResult Create()
    {
        var vm = new CourseEditVm();
        return View(vm);
    }

    // POST: /ManagerCourses/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CourseEditVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var course = new Course
        {
            Id = Guid.NewGuid(),
            Title = vm.Title.Trim(),
            Description = vm.Description?.Trim(),
            IsPublished = vm.IsPublished,
            CreatedAt = DateTime.UtcNow
        };

        _db.Courses.Add(course);
        await _db.SaveChangesAsync();

        // після створення — одразу на сторінку призначення людей
        return RedirectToAction(nameof(People), new { id = course.Id });
    }

    // GET: /ManagerCourses/Edit/{id}
    public async Task<IActionResult> Edit(Guid id)
    {
        var course = await _db.Courses.FirstOrDefaultAsync(x => x.Id == id);
        if (course == null) return NotFound();

        var vm = new CourseEditVm
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            IsPublished = course.IsPublished
        };

        return View(vm);
    }

    // POST: /ManagerCourses/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CourseEditVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var course = await _db.Courses.FirstOrDefaultAsync(x => x.Id == vm.Id);
        if (course == null) return NotFound();

        course.Title = vm.Title.Trim();
        course.Description = vm.Description?.Trim();
        course.IsPublished = vm.IsPublished;

        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // GET: /ManagerCourses/Delete/{id}
    public async Task<IActionResult> Delete(Guid id)
    {
        var course = await _db.Courses.FirstOrDefaultAsync(x => x.Id == id);
        if (course == null) return NotFound();

        var vm = new CourseEditVm
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            IsPublished = course.IsPublished
        };

        return View(vm);
    }

    // POST: /ManagerCourses/DeleteConfirmed
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var course = await _db.Courses.FirstOrDefaultAsync(x => x.Id == id);
        if (course == null) return NotFound();

        _db.Courses.Remove(course);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // =========================
    // PEOPLE (Teacher + Students)
    // =========================

    // GET: /ManagerCourses/People/{id}
    public async Task<IActionResult> People(Guid id)
    {
        var course = await _db.Courses.FirstOrDefaultAsync(x => x.Id == id);
        if (course == null) return NotFound();

        // current teacher (беремо 1)
        var currentTeacherId = await _db.CourseTeachers
            .Where(x => x.CourseId == id)
            .Select(x => x.TeacherUserId)
            .FirstOrDefaultAsync();

        // current students
        var currentStudentIds = await _db.CourseStudents
            .Where(x => x.CourseId == id)
            .Select(x => x.StudentUserId)
            .ToListAsync();

        // available teachers & students
        var teachers = await _userManager.GetUsersInRoleAsync(RoleNames.Teacher);
        var students = await _userManager.GetUsersInRoleAsync(RoleNames.Student);

        string Display(ApplicationUser u)
        {
            var first = (u.FirstName ?? "").Trim();
            var last = (u.LastName ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(first) && !string.IsNullOrWhiteSpace(last))
                return $"{first} {last}";
            if (!string.IsNullOrWhiteSpace(first)) return first;
            return u.Email ?? u.UserName ?? u.Id;
        }

        var vm = new CoursePeopleVm
        {
            CourseId = course.Id,
            CourseTitle = course.Title,
            TeacherUserId = currentTeacherId,
            SelectedStudentIds = currentStudentIds,

            Teachers = teachers
                .OrderBy(Display)
                .Select(t => new SelectListItem(Display(t), t.Id))
                .ToList(),

            Students = students
                .OrderBy(Display)
                .Select(s => new SelectListItem(Display(s), s.Id))
                .ToList()
        };

        return View(vm);
    }

    // POST: /ManagerCourses/People
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> People(CoursePeopleVm vm)
    {
        var course = await _db.Courses.FirstOrDefaultAsync(x => x.Id == vm.CourseId);
        if (course == null) return NotFound();

        // 1) TEACHER: робимо 1 teacher на курс
        var currentTeachers = await _db.CourseTeachers
            .Where(x => x.CourseId == vm.CourseId)
            .ToListAsync();

        _db.CourseTeachers.RemoveRange(currentTeachers);

        if (!string.IsNullOrWhiteSpace(vm.TeacherUserId))
        {
            _db.CourseTeachers.Add(new CourseTeacher
            {
                CourseId = vm.CourseId,
                TeacherUserId = vm.TeacherUserId!
            });
        }

        // 2) STUDENTS: синхронізація (add/remove)
        var currentStudents = await _db.CourseStudents
            .Where(x => x.CourseId == vm.CourseId)
            .ToListAsync();

        var currentIds = currentStudents.Select(x => x.StudentUserId).ToHashSet();
        var newIds = (vm.SelectedStudentIds ?? new List<string>()).ToHashSet();

        // remove missing
        var toRemove = currentStudents.Where(x => !newIds.Contains(x.StudentUserId)).ToList();
        _db.CourseStudents.RemoveRange(toRemove);

        // add new
        var toAdd = newIds.Where(id => !currentIds.Contains(id));
        foreach (var studentId in toAdd)
        {
            _db.CourseStudents.Add(new CourseStudent
            {
                CourseId = vm.CourseId,
                StudentUserId = studentId,
                EnrolledAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
