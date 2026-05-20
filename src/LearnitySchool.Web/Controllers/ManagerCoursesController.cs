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

    private static string Display(ApplicationUser u)
    {
        var first = (u.FirstName ?? string.Empty).Trim();
        var last = (u.LastName ?? string.Empty).Trim();

        if (!string.IsNullOrWhiteSpace(first) && !string.IsNullOrWhiteSpace(last))
            return $"{first} {last} — {u.Email}";

        if (!string.IsNullOrWhiteSpace(first))
            return $"{first} — {u.Email}";

        return u.Email ?? u.UserName ?? u.Id;
    }

    private async Task<List<SelectListItem>> GetRoleOptionsAsync(string roleName)
    {
        var users = await _userManager.GetUsersInRoleAsync(roleName);

        return users
            .OrderBy(Display)
            .Select(u => new SelectListItem(Display(u), u.Id))
            .ToList();
    }

    private async Task PopulateCloneListsAsync(CourseCloneVm vm)
    {
        vm.Teachers = await GetRoleOptionsAsync(RoleNames.Teacher);
        vm.Students = await GetRoleOptionsAsync(RoleNames.Student);
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

        var managerId = _userManager.GetUserId(User);

        var course = new Course
        {
            Id = Guid.NewGuid(),
            Title = vm.Title.Trim(),
            Description = vm.Description?.Trim(),
            IsPublished = vm.IsPublished,
            CreatedAt = DateTime.UtcNow,
            ManagerUserId = managerId
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

        // ✅ якщо раніше не було — запишемо хто зараз менеджер
        if (string.IsNullOrWhiteSpace(course.ManagerUserId))
            course.ManagerUserId = _userManager.GetUserId(User);

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
    // CLONE COURSE
    // =========================

    // GET: /ManagerCourses/Clone/{id}
    public async Task<IActionResult> Clone(Guid id)
    {
        var source = await _db.Courses
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new
            {
                c.Id,
                c.Title,
                c.Description,
                c.IsPublished,
                LessonsCount = c.Lessons.Count(),
                TasksCount = c.Lessons.SelectMany(l => l.Tasks).Count(),
                TeacherUserId = c.Teachers.Select(t => t.TeacherUserId).FirstOrDefault()
            })
            .FirstOrDefaultAsync();

        if (source == null) return NotFound();

        var vm = new CourseCloneVm
        {
            SourceCourseId = source.Id,
            SourceCourseTitle = source.Title,
            Title = $"{source.Title} (копія)",
            Description = source.Description,
            IsPublished = source.IsPublished,
            LessonsCount = source.LessonsCount,
            TasksCount = source.TasksCount,
            TeacherUserId = source.TeacherUserId,
            SelectedStudentIds = new List<string>()
        };

        await PopulateCloneListsAsync(vm);
        return View(vm);
    }

    // POST: /ManagerCourses/Clone
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clone(CourseCloneVm vm)
    {
        var sourceCourse = await _db.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == vm.SourceCourseId);

        if (sourceCourse == null) return NotFound();

        var lessons = await _db.Lessons
            .AsNoTracking()
            .Where(l => l.CourseId == vm.SourceCourseId)
            .OrderBy(l => l.Order)
            .ToListAsync();

        var lessonIds = lessons.Select(l => l.Id).ToList();

        var tasks = await _db.LessonTasks
            .AsNoTracking()
            .Where(t => lessonIds.Contains(t.LessonId))
            .OrderBy(t => t.Order)
            .ToListAsync();

        var taskIds = tasks.Select(t => t.Id).ToList();

        var practiceTasks = await _db.PracticeTasks
            .AsNoTracking()
            .Where(p => taskIds.Contains(p.LessonTaskId))
            .ToListAsync();

        var questions = await _db.QuizQuestions
            .AsNoTracking()
            .Where(q => taskIds.Contains(q.TaskId))
            .OrderBy(q => q.Order)
            .ToListAsync();

        var questionIds = questions.Select(q => q.Id).ToList();

        var options = await _db.QuizOptions
            .AsNoTracking()
            .Where(o => questionIds.Contains(o.QuestionId))
            .OrderBy(o => o.Order)
            .ToListAsync();

        var schedules = await _db.CourseSchedules
            .AsNoTracking()
            .Where(s => s.CourseId == vm.SourceCourseId)
            .ToListAsync();

        vm.SourceCourseTitle = sourceCourse.Title;
        vm.LessonsCount = lessons.Count;
        vm.TasksCount = tasks.Count;

        if (!ModelState.IsValid)
        {
            await PopulateCloneListsAsync(vm);
            return View(vm);
        }

        var newCourseId = Guid.NewGuid();
        var managerId = _userManager.GetUserId(User);

        var newCourse = new Course
        {
            Id = newCourseId,
            Title = vm.Title.Trim(),
            Description = vm.Description?.Trim(),
            IsPublished = vm.IsPublished,
            CreatedAt = DateTime.UtcNow,
            ManagerUserId = managerId
        };

        _db.Courses.Add(newCourse);

        foreach (var schedule in schedules)
        {
            _db.CourseSchedules.Add(new CourseSchedule
            {
                Id = Guid.NewGuid(),
                CourseId = newCourseId,
                DayOfWeek = schedule.DayOfWeek,
                StartTime = schedule.StartTime,
                Duration = schedule.Duration
            });
        }

        var lessonMap = new Dictionary<Guid, Guid>();
        foreach (var lesson in lessons)
        {
            var newLessonId = Guid.NewGuid();
            lessonMap[lesson.Id] = newLessonId;

            _db.Lessons.Add(new Lesson
            {
                Id = newLessonId,
                CourseId = newCourseId,
                Title = lesson.Title,
                Description = lesson.Description,
                Order = lesson.Order,
                Content = lesson.Content,
                IsPublished = lesson.IsPublished,
                PriceAmount = lesson.PriceAmount,
                Currency = lesson.Currency
            });
        }

        var taskMap = new Dictionary<Guid, Guid>();
        foreach (var task in tasks)
        {
            if (!lessonMap.TryGetValue(task.LessonId, out var newLessonId)) continue;

            var newTaskId = Guid.NewGuid();
            taskMap[task.Id] = newTaskId;

            _db.LessonTasks.Add(new LessonTask
            {
                Id = newTaskId,
                LessonId = newLessonId,
                Order = task.Order,
                Title = task.Title,
                Description = task.Description,
                Type = task.Type,
                IsPublished = task.IsPublished,
                AssessmentMode = task.AssessmentMode,
                QuizType = task.QuizType
            });
        }

        foreach (var practice in practiceTasks)
        {
            if (!taskMap.TryGetValue(practice.LessonTaskId, out var newTaskId)) continue;

            _db.PracticeTasks.Add(new PracticeTask
            {
                Id = Guid.NewGuid(),
                LessonTaskId = newTaskId,
                Statement = practice.Statement ?? string.Empty,
                StarterHtml = practice.StarterHtml ?? string.Empty,
                StarterCss = practice.StarterCss ?? string.Empty,
                StarterJs = practice.StarterJs ?? string.Empty,
                ReferenceHtml = practice.ReferenceHtml ?? string.Empty,
                ReferenceCss = practice.ReferenceCss ?? string.Empty,
                ReferenceJs = practice.ReferenceJs ?? string.Empty,
                SimilarityThreshold = Math.Max(practice.SimilarityThreshold, 95)
            });
        }

        var questionMap = new Dictionary<Guid, Guid>();
        foreach (var question in questions)
        {
            if (!taskMap.TryGetValue(question.TaskId, out var newTaskId)) continue;

            var newQuestionId = Guid.NewGuid();
            questionMap[question.Id] = newQuestionId;

            _db.QuizQuestions.Add(new QuizQuestion
            {
                Id = newQuestionId,
                TaskId = newTaskId,
                Text = question.Text,
                Order = question.Order,
                IsPublished = question.IsPublished
            });
        }

        foreach (var option in options)
        {
            if (!questionMap.TryGetValue(option.QuestionId, out var newQuestionId)) continue;

            _db.QuizOptions.Add(new QuizOption
            {
                Id = Guid.NewGuid(),
                QuestionId = newQuestionId,
                Text = option.Text,
                IsCorrect = option.IsCorrect,
                Order = option.Order
            });
        }

        if (!string.IsNullOrWhiteSpace(vm.TeacherUserId))
        {
            _db.CourseTeachers.Add(new CourseTeacher
            {
                CourseId = newCourseId,
                TeacherUserId = vm.TeacherUserId
            });
        }

        foreach (var studentId in (vm.SelectedStudentIds ?? new List<string>()).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct())
        {
            _db.CourseStudents.Add(new CourseStudent
            {
                CourseId = newCourseId,
                StudentUserId = studentId,
                EnrolledAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();

        TempData["Success"] = $"Курс “{sourceCourse.Title}” скопійовано як “{newCourse.Title}”.";
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
