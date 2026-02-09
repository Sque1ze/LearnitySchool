using LearnitySchool.Application.Common;
using LearnitySchool.Infrastructure.Identity;
using LearnitySchool.Infrastructure.Persistence;
using LearnitySchool.Web.ViewModels.Teacher.Groups;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearnitySchool.Web.Controllers;

[Authorize(Roles = RoleNames.Teacher)]
public class TeacherGroupsController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public TeacherGroupsController(AppDbContext db, UserManager<ApplicationUser> userManager)
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
        return u.Email ?? u.UserName ?? u.Id;
    }

    // GET: /TeacherGroups
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] TeacherGroupsVm vm)
    {
        var currentTeacherId = _userManager.GetUserId(User);

        // dropdowns
        var teachers = await _userManager.GetUsersInRoleAsync(RoleNames.Teacher);
        var managers = await _userManager.GetUsersInRoleAsync(RoleNames.Manager);

        vm.Teachers = teachers
            .OrderBy(Display)
            .Select(t => new FilterOptionVm { Id = t.Id, Text = Display(t) })
            .ToList();

        vm.Managers = managers
            .OrderBy(Display)
            .Select(m => new FilterOptionVm { Id = m.Id, Text = Display(m) })
            .ToList();

        // ✅ базовий query: показуємо лише курси, де призначений цей teacher
        var q = _db.Courses
            .AsNoTracking()
            .Where(c => c.Teachers.Any(t => t.TeacherUserId == currentTeacherId))
            .AsQueryable();

        // 🔎 Назва
        if (!string.IsNullOrWhiteSpace(vm.Q))
        {
            var term = vm.Q.Trim();
            q = q.Where(c => c.Title.Contains(term));
        }

        // 👤 Викладач (фільтр по конкретному teacher)
        if (!string.IsNullOrWhiteSpace(vm.TeacherId))
        {
            var tid = vm.TeacherId.Trim();
            q = q.Where(c => c.Teachers.Any(t => t.TeacherUserId == tid));
        }

        // 👤 Менеджер
        if (!string.IsNullOrWhiteSpace(vm.ManagerId))
        {
            var mid = vm.ManagerId.Trim();
            q = q.Where(c => c.ManagerUserId == mid);
        }

        // 👥 К-ть учнів (точна цифра)
        if (vm.StudentsCount.HasValue)
        {
            var count = vm.StudentsCount.Value;
            q = q.Where(c => c.Students.Count() == count);
        }

        // витягуємо дані
        var items = await q
            .OrderBy(c => c.Title)
            .Select(c => new
            {
                c.Id,
                c.Title,
                c.IsPublished,
                StudentsCount = c.Students.Count(),
                TeacherId = c.Teachers.Select(t => t.TeacherUserId).FirstOrDefault(),
                c.ManagerUserId
            })
            .ToListAsync();

        // мапимо імена users
        var teacherIds = items.Select(x => x.TeacherId).Where(x => x != null).Distinct().ToList()!;
        var managerIds = items.Select(x => x.ManagerUserId).Where(x => x != null).Distinct().ToList()!;

        var people = await _db.Users
            .AsNoTracking()
            .Where(u => teacherIds.Contains(u.Id) || managerIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => Display(u));

        vm.Items = items.Select(x => new TeacherGroupRowVm
        {
            CourseId = x.Id,
            Title = x.Title,
            StudentsCount = x.StudentsCount,
            TeacherName = x.TeacherId != null && people.TryGetValue(x.TeacherId, out var tn) ? tn : "—",
            ManagerName = x.ManagerUserId != null && people.TryGetValue(x.ManagerUserId, out var mn) ? mn : null,
            IsPublished = x.IsPublished,
            Format = "Онлайн"
        }).ToList();

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Success(Guid id, Guid? lessonId)
    {
        var teacherId = _userManager.GetUserId(User);

        // курс існує і вчитель призначений на курс
        var course = await _db.Courses
            .AsNoTracking()
            .Where(c => c.Id == id && c.Teachers.Any(t => t.TeacherUserId == teacherId))
            .Select(c => new { c.Id, c.Title })
            .FirstOrDefaultAsync();

        if (course == null) return NotFound();

        // уроки курсу
        var lessons = await _db.Lessons
            .AsNoTracking()
            .Where(l => l.CourseId == id)
            .OrderBy(l => l.Order)
            .Select(l => new { l.Id, l.Title })
            .ToListAsync();

        // студенти курсу
        var studentIds = await _db.CourseStudents
            .AsNoTracking()
            .Where(cs => cs.CourseId == id)
            .Select(cs => cs.StudentUserId)
            .ToListAsync();

        // всі published tasks курсу (для підрахунку прогресу)
        var tasks = await _db.LessonTasks
            .AsNoTracking()
            .Where(t => t.Lesson.CourseId == id && t.IsPublished)
            .Select(t => new { t.Id, t.LessonId })
            .ToListAsync();

        // якщо lessonId не передали — вибираємо перший урок
        var selectedLessonId = lessonId ?? lessons.FirstOrDefault()?.Id;

        // прогрес (тільки completed, щоб не тягнути зайве)
        var taskIds = tasks.Select(t => t.Id).ToList();

        var completed = await _db.StudentTaskProgresses
            .AsNoTracking()
            .Where(p =>
                studentIds.Contains(p.StudentUserId) &&
                taskIds.Contains(p.TaskId) &&
                p.IsCompleted)
            .Select(p => new { p.StudentUserId, p.TaskId })
            .ToListAsync();

        // ---------- підрахунки ----------
        var studentsCount = studentIds.Count;

        var tasksByLesson = tasks
            .GroupBy(x => x.LessonId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Id).ToList());

        // completed по taskId -> lessonId (через map)
        var taskToLesson = tasks.ToDictionary(x => x.Id, x => x.LessonId);

        // completed count per lesson (всього)
        var completedPerLesson = new Dictionary<Guid, int>();
        foreach (var c in completed)
        {
            if (!taskToLesson.TryGetValue(c.TaskId, out var lId)) continue;
            completedPerLesson[lId] = completedPerLesson.TryGetValue(lId, out var v) ? v + 1 : 1;
        }

        // список уроків з % (середній прогрес групи по уроку)
        var lessonCards = new List<LessonSuccessVm>();
        foreach (var l in lessons)
        {
            tasksByLesson.TryGetValue(l.Id, out var lessonTaskIds);
            var totalTasks = lessonTaskIds?.Count ?? 0;

            int percent = 0;
            if (studentsCount > 0 && totalTasks > 0)
            {
                completedPerLesson.TryGetValue(l.Id, out var done);
                var total = studentsCount * totalTasks;
                percent = (int)Math.Round(done * 100.0 / total);
            }

            lessonCards.Add(new LessonSuccessVm
            {
                LessonId = l.Id,
                Title = l.Title,
                Percent = Math.Clamp(percent, 0, 100),
                TasksCount = totalTasks
            });
        }

        // учні + імена
        var users = await _db.Users
            .AsNoTracking()
            .Where(u => studentIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FirstName, u.LastName, u.Email, u.UserName })
            .ToListAsync();

        string DisplayName(dynamic u)
        {
            var first = (u.FirstName ?? "").Trim();
            var last = (u.LastName ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(first) && !string.IsNullOrWhiteSpace(last)) return $"{first} {last}";
            if (!string.IsNullOrWhiteSpace(first)) return first;
            return u.Email ?? u.UserName ?? u.Id;
        }

        var nameById = users.ToDictionary(x => x.Id, x => DisplayName(x));

        // прогрес по вибраному уроку
        var studentRows = new List<StudentSuccessRowVm>();

        var selectedTasks = (selectedLessonId != null && tasksByLesson.TryGetValue(selectedLessonId.Value, out var list))
            ? list
            : new List<Guid>();

        // completed by student for selected lesson
        var doneByStudent = new Dictionary<string, int>();
        if (selectedTasks.Count > 0)
        {
            var selectedSet = selectedTasks.ToHashSet();
            foreach (var c in completed)
            {
                if (!selectedSet.Contains(c.TaskId)) continue;
                doneByStudent[c.StudentUserId] = doneByStudent.TryGetValue(c.StudentUserId, out var v) ? v + 1 : 1;
            }
        }

        foreach (var sid in studentIds)
        {
            var done = doneByStudent.TryGetValue(sid, out var v) ? v : 0;
            var total = selectedTasks.Count;

            var percent = (total > 0) ? (int)Math.Round(done * 100.0 / total) : 0;

            studentRows.Add(new StudentSuccessRowVm
            {
                StudentId = sid,
                StudentName = nameById.TryGetValue(sid, out var n) ? n : sid,
                Percent = Math.Clamp(percent, 0, 100)
            });
        }

        // відсортуємо: найвищі зверху
        studentRows = studentRows.OrderByDescending(x => x.Percent).ThenBy(x => x.StudentName).ToList();

        var vm = new TeacherGroupSuccessVm
        {
            CourseId = course.Id,
            CourseTitle = course.Title,
            SelectedLessonId = selectedLessonId,
            Lessons = lessonCards,
            Students = studentRows
        };

        return View(vm);
    }

    // GET: /TeacherGroups/Details/{id}
    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var currentTeacherId = _userManager.GetUserId(User);

        // доступ: лише якщо teacher призначений на курс
        var allowed = await _db.CourseTeachers
            .AsNoTracking()
            .AnyAsync(ct => ct.CourseId == id && ct.TeacherUserId == currentTeacherId);

        if (!allowed) return Forbid();

        var course = await _db.Courses
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new
            {
                c.Id,
                c.Title,
                c.Description,
                c.IsPublished,
                c.CreatedAt,
                c.ManagerUserId,
                TeacherId = c.Teachers.Select(t => t.TeacherUserId).FirstOrDefault(),
                StudentsCount = c.Students.Count(),
                Schedules = c.Schedules
                    .OrderBy(s => s.DayOfWeek)
                    .ThenBy(s => s.StartTime)
                    .Select(s => new { s.DayOfWeek, s.StartTime, s.Duration })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (course == null) return NotFound();

        // students list
        var studentIds = await _db.CourseStudents
            .AsNoTracking()
            .Where(cs => cs.CourseId == id)
            .Select(cs => cs.StudentUserId)
            .ToListAsync();

        var students = await _db.Users
            .AsNoTracking()
            .Where(u => studentIds.Contains(u.Id))
            .Select(u => new TeacherGroupStudentRowVm
            {
                UserId = u.Id,
                FullName = (u.FirstName + " " + u.LastName).Trim(),
                Email = u.Email,
                Age = u.Age
            })
            .OrderBy(x => x.FullName)
            .ToListAsync();

        // resolve names teacher/manager
        var ids = new List<string>();
        if (!string.IsNullOrWhiteSpace(course.TeacherId)) ids.Add(course.TeacherId!);
        if (!string.IsNullOrWhiteSpace(course.ManagerUserId)) ids.Add(course.ManagerUserId!);

        var people = await _db.Users
            .AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => Display(u));

        var teacherName = (!string.IsNullOrWhiteSpace(course.TeacherId) && people.TryGetValue(course.TeacherId!, out var tn))
            ? tn
            : "—";

        var managerName = (!string.IsNullOrWhiteSpace(course.ManagerUserId) && people.TryGetValue(course.ManagerUserId!, out var mn))
            ? mn
            : null;

        static string DayUa(DayOfWeek d) => d switch
        {
            DayOfWeek.Monday => "Пн",
            DayOfWeek.Tuesday => "Вт",
            DayOfWeek.Wednesday => "Ср",
            DayOfWeek.Thursday => "Чт",
            DayOfWeek.Friday => "Пт",
            DayOfWeek.Saturday => "Сб",
            DayOfWeek.Sunday => "Нд",
            _ => d.ToString()
        };

        var scheduleText = (course.Schedules?.Count ?? 0) == 0
            ? "—"
            : string.Join(" • ", course.Schedules.Select(s =>
                $"{DayUa(s.DayOfWeek)} {s.StartTime:hh\\:mm} ({s.Duration:hh\\:mm})"));

        var vm = new TeacherGroupDetailsVm
        {
            CourseId = course.Id,
            Title = course.Title,
            Description = course.Description ?? "",
            IsPublished = course.IsPublished,
            Format = "Онлайн",

            StudentsCount = course.StudentsCount,

            TeacherName = teacherName,
            ManagerName = managerName,

            CreatedAtUtc = course.CreatedAt,
            ScheduleText = scheduleText,

            Students = students
        };

        return View(vm);
    }
}
