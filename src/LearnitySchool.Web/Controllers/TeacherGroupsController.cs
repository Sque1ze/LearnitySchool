using LearnitySchool.Application.Common;
using LearnitySchool.Domain.Entities;
using LearnitySchool.Domain.Enums;
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

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] TeacherGroupsVm vm)
    {
        var currentTeacherId = _userManager.GetUserId(User);

        var q = _db.Courses
            .AsNoTracking()
            .Where(c => c.Teachers.Any(t => t.TeacherUserId == currentTeacherId))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(vm.Q))
        {
            var term = vm.Q.Trim();
            q = q.Where(c => c.Title.Contains(term));
        }

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

        var teacherIds = items.Select(x => x.TeacherId).Where(x => x != null).Distinct().ToList()!;
        var managerIds = items.Select(x => x.ManagerUserId).Where(x => x != null).Distinct().ToList()!;

        var peopleList = await _db.Users
            .AsNoTracking()
            .Where(u => teacherIds.Contains(u.Id) || managerIds.Contains(u.Id))
            .ToListAsync();

        var people = peopleList.ToDictionary(u => u.Id, Display);

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
    public async Task<IActionResult> Details(Guid id, Guid? lessonId = null)
    {
        var teacherUserId = _userManager.GetUserId(User);

        var allowed = await _db.CourseTeachers
            .AsNoTracking()
            .AnyAsync(x => x.CourseId == id && x.TeacherUserId == teacherUserId);

        if (!allowed) return Forbid();

        var course = await _db.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null) return NotFound();

        var lessons = await _db.Lessons
            .AsNoTracking()
            .Where(l => l.CourseId == id)
            .OrderBy(l => l.Order)
            .Select(l => new { l.Id, l.Title, l.Order })
            .ToListAsync();

        var selectedLessonId = lessonId ?? lessons.FirstOrDefault()?.Id;

        var accessMap = await _db.LessonAccesses
            .AsNoTracking()
            .Where(x => x.CourseId == id)
            .ToDictionaryAsync(x => x.LessonId, x => x.IsOpen);

        var teacherId = await _db.CourseTeachers
            .AsNoTracking()
            .Where(x => x.CourseId == id)
            .Select(x => x.TeacherUserId)
            .FirstOrDefaultAsync();

        var managerId = course.ManagerUserId;

        var schedules = await _db.CourseSchedules
            .AsNoTracking()
            .Where(s => s.CourseId == id)
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .Select(s => new { s.DayOfWeek, s.StartTime, s.Duration })
            .ToListAsync();

        string scheduleText = schedules.Count == 0
            ? "—"
            : string.Join(", ", schedules.Select(s =>
                $"{s.DayOfWeek} {s.StartTime:hh\\:mm} ({s.Duration:hh\\:mm})"));

        var studentIds = await _db.CourseStudents
            .AsNoTracking()
            .Where(cs => cs.CourseId == id)
            .Select(cs => cs.StudentUserId)
            .ToListAsync();

        var peopleIds = new List<string>();
        if (!string.IsNullOrWhiteSpace(teacherId)) peopleIds.Add(teacherId!);
        if (!string.IsNullOrWhiteSpace(managerId)) peopleIds.Add(managerId!);
        peopleIds.AddRange(studentIds);
        peopleIds = peopleIds.Distinct().ToList();

        var peopleList = await _db.Users
            .AsNoTracking()
            .Where(u => peopleIds.Contains(u.Id))
            .ToListAsync();

        var people = peopleList.ToDictionary(u => u.Id, u => new
        {
            Name = Display(u),
            u.Email,
            u.Age
        });

        static string Initials(string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "•";
            var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpperInvariant();
            return (parts[0].Substring(0, 1) + parts[1].Substring(0, 1)).ToUpperInvariant();
        }

        var studentsVm = studentIds
            .Select(sid =>
            {
                people.TryGetValue(sid, out var p);
                return new TeacherGroupStudentRowVm
                {
                    UserId = sid,
                    FullName = p?.Name ?? "—",
                    Email = p?.Email,
                    Age = p?.Age
                };
            })
            .OrderBy(x => x.FullName)
            .ToList();

        var tasks = await _db.LessonTasks
            .AsNoTracking()
            .Where(t => t.Lesson.CourseId == id && t.IsPublished)
            .Select(t => new { t.Id, t.LessonId })
            .ToListAsync();

        var tasksByLesson = tasks
            .GroupBy(t => t.LessonId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Id).ToList());

        var taskIds = tasks.Select(t => t.Id).ToList();

        var progresses = await _db.StudentTaskProgresses
            .AsNoTracking()
            .Where(p => studentIds.Contains(p.StudentUserId) && taskIds.Contains(p.TaskId))
            .Select(p => new { p.StudentUserId, p.TaskId, p.IsCompleted })
            .ToListAsync();

        var completedSet = progresses
            .Where(x => x.IsCompleted)
            .Select(x => (x.StudentUserId, x.TaskId))
            .ToHashSet();

        var successLessons = new List<TeacherGroupDetailsVm.SuccessLessonVm>();
        var successStudents = new List<TeacherGroupDetailsVm.SuccessStudentVm>();

        foreach (var lesson in lessons)
        {
            tasksByLesson.TryGetValue(lesson.Id, out var lessonTaskIds);
            lessonTaskIds ??= new List<Guid>();
            var taskCount = lessonTaskIds.Count;

            var isOpen = accessMap.TryGetValue(lesson.Id, out var openFlag) && openFlag;

            if (taskCount == 0 || studentIds.Count == 0)
            {
                successLessons.Add(new TeacherGroupDetailsVm.SuccessLessonVm
                {
                    LessonId = lesson.Id,
                    Title = lesson.Title,
                    DateText = "",
                    Percent = 0,
                    IsSelected = selectedLessonId.HasValue && selectedLessonId.Value == lesson.Id,
                    IsOpen = isOpen
                });

                foreach (var sid in studentIds)
                {
                    people.TryGetValue(sid, out var p);
                    var name = p?.Name ?? "—";

                    successStudents.Add(new TeacherGroupDetailsVm.SuccessStudentVm
                    {
                        LessonId = lesson.Id,
                        StudentName = name,
                        Initials = Initials(name),
                        SubText = p?.Email ?? "",
                        Percent = 0
                    });
                }

                continue;
            }

            var perStudentPercents = new List<int>();

            foreach (var sid in studentIds)
            {
                var completed = 0;
                foreach (var tid in lessonTaskIds)
                {
                    if (completedSet.Contains((sid, tid)))
                        completed++;
                }

                var percent = (int)Math.Round((completed * 100.0) / taskCount);
                perStudentPercents.Add(percent);

                people.TryGetValue(sid, out var p);
                var name = p?.Name ?? "—";

                successStudents.Add(new TeacherGroupDetailsVm.SuccessStudentVm
                {
                    LessonId = lesson.Id,
                    StudentName = name,
                    Initials = Initials(name),
                    SubText = p?.Email ?? "",
                    Percent = percent
                });
            }

            var lessonPercent = (int)Math.Round(perStudentPercents.Average());

            successLessons.Add(new TeacherGroupDetailsVm.SuccessLessonVm
            {
                LessonId = lesson.Id,
                Title = lesson.Title,
                DateText = "",
                Percent = lessonPercent,
                IsSelected = selectedLessonId.HasValue && selectedLessonId.Value == lesson.Id,
                IsOpen = isOpen
            });
        }

        var attendanceRows = await _db.StudentLessonAttendances
            .AsNoTracking()
            .Where(a => a.CourseId == id && studentIds.Contains(a.StudentUserId))
            .Select(a => new { a.StudentUserId, a.LessonId, a.Status })
            .ToListAsync();

        var attendanceMap = attendanceRows.ToDictionary(
            x => (x.StudentUserId, x.LessonId),
            x => (int)x.Status);

        var attendanceLessons = lessons.Select(l => new TeacherGroupDetailsVm.AttendanceLessonVm
        {
            LessonId = l.Id,
            Order = l.Order,
            Title = l.Title
        }).ToList();

        var attendanceStudents = new List<TeacherGroupDetailsVm.AttendanceStudentVm>();

        foreach (var sid in studentIds)
        {
            people.TryGetValue(sid, out var p);
            var name = p?.Name ?? "—";

            var statuses = new List<int>(attendanceLessons.Count);
            foreach (var les in attendanceLessons)
            {
                if (attendanceMap.TryGetValue((sid, les.LessonId), out var st))
                    statuses.Add(st);
                else
                    statuses.Add(0);
            }

            attendanceStudents.Add(new TeacherGroupDetailsVm.AttendanceStudentVm
            {
                StudentUserId = sid,
                StudentName = name,
                Initials = Initials(name),
                SubText = p?.Email ?? "",
                Statuses = statuses
            });
        }

        attendanceStudents = attendanceStudents.OrderBy(x => x.StudentName).ToList();

        var vm = new TeacherGroupDetailsVm
        {
            CourseId = course.Id,
            Title = course.Title,
            Description = course.Description,
            IsPublished = course.IsPublished,
            Format = "Онлайн",

            StudentsCount = studentIds.Count,
            TeacherName = (!string.IsNullOrWhiteSpace(teacherId) && people.TryGetValue(teacherId!, out var tP)) ? tP.Name : "—",
            ManagerName = (!string.IsNullOrWhiteSpace(managerId) && people.TryGetValue(managerId!, out var mP)) ? mP.Name : null,
            ScheduleText = scheduleText,
            CreatedAtUtc = course.CreatedAt,

            Students = studentsVm,
            SuccessLessons = successLessons,
            SuccessStudents = successStudents,
            AttendanceLessons = attendanceLessons,
            AttendanceStudents = attendanceStudents
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLessonAccess(Guid courseId, Guid lessonId)
    {
        var userId = _userManager.GetUserId(User);

        var allowed = await _db.CourseTeachers
            .AnyAsync(x => x.CourseId == courseId && x.TeacherUserId == userId);

        if (!allowed) return Forbid();

        var gate = await _db.LessonAccesses
            .FirstOrDefaultAsync(x => x.CourseId == courseId && x.LessonId == lessonId);

        if (gate == null)
        {
            gate = new LessonAccess
            {
                CourseId = courseId,
                LessonId = lessonId,
                IsOpen = true,
                OpenedAtUtc = DateTime.UtcNow,
                OpenedByUserId = userId
            };
            _db.LessonAccesses.Add(gate);
        }
        else
        {
            gate.IsOpen = !gate.IsOpen;
            gate.OpenedAtUtc = gate.IsOpen ? DateTime.UtcNow : null;
            gate.OpenedByUserId = gate.IsOpen ? userId : null;
        }

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = courseId });
    }

    [HttpGet]
    public async Task<IActionResult> StudentTasksPanel(Guid courseId, string studentUserId)
    {
        var teacherUserId = _userManager.GetUserId(User);

        var allowed = await _db.CourseTeachers
            .AsNoTracking()
            .AnyAsync(x => x.CourseId == courseId && x.TeacherUserId == teacherUserId);

        if (!allowed) return Forbid();

        var studentInCourse = await _db.CourseStudents
            .AsNoTracking()
            .AnyAsync(x => x.CourseId == courseId && x.StudentUserId == studentUserId);

        if (!studentInCourse) return NotFound();

        var studentUser = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == studentUserId);

        var student = studentUser == null ? null : new { Name = Display(studentUser) };

        var lessons = await _db.Lessons
            .AsNoTracking()
            .Where(l => l.CourseId == courseId && l.IsPublished)
            .OrderBy(l => l.Order)
            .Select(l => new { l.Id, l.Order, l.Title })
            .ToListAsync();

        var tasks = await _db.LessonTasks
            .AsNoTracking()
            .Where(t => t.Lesson.CourseId == courseId && t.IsPublished)
            .OrderBy(t => t.Lesson.Order)
            .ThenBy(t => t.Order)
            .Select(t => new { t.Id, t.LessonId, t.Order, t.Title, t.AssessmentMode })
            .ToListAsync();

        var taskIds = tasks.Select(t => t.Id).ToList();

        var completed = await _db.StudentTaskProgresses
            .AsNoTracking()
            .Where(p =>
                p.StudentUserId == studentUserId &&
                p.IsCompleted &&
                taskIds.Contains(p.TaskId))
            .Select(p => p.TaskId)
            .ToListAsync();

        var completedSet = completed.ToHashSet();

        var submissions = await _db.StudentTaskSubmissions
            .AsNoTracking()
            .Where(s => s.StudentUserId == studentUserId && taskIds.Contains(s.LessonTaskId))
            .Select(s => new { s.LessonTaskId, s.Status })
            .ToListAsync();

        var submissionMap = submissions.ToDictionary(x => x.LessonTaskId, x => x.Status);

        var quizAttempts = await _db.StudentQuizAttempts
            .AsNoTracking()
            .Where(a => a.StudentUserId == studentUserId && taskIds.Contains(a.TaskId))
            .Select(a => new { a.TaskId, a.ScorePercent })
            .ToListAsync();

        var quizAttemptMap = quizAttempts.ToDictionary(x => x.TaskId, x => x.ScorePercent);

        var tasksByLesson = tasks
            .GroupBy(t => t.LessonId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var vm = new TeacherStudentTasksVm
        {
            CourseId = courseId,
            StudentUserId = studentUserId,
            StudentName = student?.Name ?? "—",
            Lessons = lessons.Select(l =>
            {
                tasksByLesson.TryGetValue(l.Id, out var taskList);

                var squares = new List<TeacherStudentTasksVm.TaskSquareVm>();
                if (taskList != null)
                {
                    squares = taskList.Select(t =>
                    {
                        var hasQuizAttempt = quizAttemptMap.TryGetValue(t.Id, out var quizScore);

                        return new TeacherStudentTasksVm.TaskSquareVm
                        {
                            TaskId = t.Id,
                            Order = t.Order,
                            Title = t.Title ?? string.Empty,
                            IsCompleted = completedSet.Contains(t.Id),
                            AssessmentMode = t.AssessmentMode,
                            SubmissionStatus = submissionMap.TryGetValue(t.Id, out var status) ? status : null,
                            HasQuizAttempt = hasQuizAttempt,
                            QuizScorePercent = hasQuizAttempt ? quizScore : null
                        };
                    }).ToList();
                }

                return new TeacherStudentTasksVm.LessonBlockVm
                {
                    LessonId = l.Id,
                    Order = l.Order,
                    Title = l.Title,
                    Tasks = squares
                };
            }).ToList()
        };

        return PartialView("_StudentTasksPanel", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetAttendanceAjax(Guid courseId, Guid lessonId, string studentUserId, int status)
    {
        var teacherId = _userManager.GetUserId(User);

        var allowed = await _db.CourseTeachers
            .AnyAsync(x => x.CourseId == courseId && x.TeacherUserId == teacherId);

        if (!allowed) return Forbid();

        var row = await _db.StudentLessonAttendances
            .FirstOrDefaultAsync(x => x.CourseId == courseId && x.LessonId == lessonId && x.StudentUserId == studentUserId);

        if (row == null)
        {
            row = new StudentLessonAttendance
            {
                Id = Guid.NewGuid(),
                CourseId = courseId,
                LessonId = lessonId,
                StudentUserId = studentUserId,
                Status = (AttendanceStatus)status,
                UpdatedByTeacherUserId = teacherId!,
                UpdatedAtUtc = DateTime.UtcNow
            };
            _db.StudentLessonAttendances.Add(row);
        }
        else
        {
            row.Status = (AttendanceStatus)status;
            row.UpdatedByTeacherUserId = teacherId!;
            row.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return Json(new { ok = true, status });
    }
}