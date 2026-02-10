using LearnitySchool.Application.Common;          // RoleNames (важливо: тільки одне!)
using LearnitySchool.Domain.Entities;
using LearnitySchool.Domain.Enums;
using LearnitySchool.Infrastructure.Identity;
using LearnitySchool.Infrastructure.Persistence;
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

                LessonsCount = _db.Lessons.Count(l => l.CourseId == c.Id && l.IsPublished),

                TotalTasks = _db.LessonTasks.Count(t =>
                    t.IsPublished &&
                    t.Lesson.IsPublished &&
                    t.Lesson.CourseId == c.Id),

                CompletedTasks = _db.StudentTaskProgresses.Count(p =>
                    p.StudentUserId == userId &&
                    p.IsCompleted &&
                    p.Task.IsPublished &&
                    p.Task.Lesson.IsPublished &&
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

        // (опц.) перевірка що студент у курсі
        var allowed = await _db.CourseStudents.AnyAsync(x => x.CourseId == courseId && x.StudentUserId == userId);
        if (!allowed) return Forbid();

        // ✅ дістаємо open/closed для уроків цього курсу з LessonAccesses
        // якщо запису нема -> урок вважаємо закритим
        var gates = await _db.LessonAccesses
            .Where(g => g.CourseId == courseId)
            .Select(g => new { g.LessonId, g.IsOpen })
            .ToDictionaryAsync(x => x.LessonId, x => x.IsOpen);

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
                    p.Task.IsPublished &&
                    p.Task.LessonId == l.Id),

                ProgressPercent = 0,

                // ✅ заповнимо після ToList
                IsOpen = false
            })
            .ToListAsync();

        foreach (var l in lessons)
        {
            l.ProgressPercent = l.TasksCount == 0
                ? 0
                : (int)Math.Round((double)l.CompletedTasks * 100 / l.TasksCount);

            // ✅ open/closed
            l.IsOpen = gates.TryGetValue(l.LessonId, out var open) && open;
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

        if (courseId == Guid.Empty) return NotFound();

        // (опц.) перевірка що студент у курсі
        var allowed = await _db.CourseStudents.AnyAsync(x =>
            x.CourseId == courseId &&
            x.StudentUserId == userId);

        if (!allowed) return Forbid();

        // ✅ gate через LessonAccesses (якщо запису нема -> false)
        var isOpen = await _db.LessonAccesses
            .Where(x => x.CourseId == courseId && x.LessonId == lessonId)
            .Select(x => x.IsOpen)
            .FirstOrDefaultAsync();

        if (!isOpen)
        {
            TempData["Error"] = "Цей урок зараз закритий викладачем.";
            return RedirectToAction(nameof(Lessons), new { courseId });
        }

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
    // COMPLETE TASK (POST) - manual mark (може лишитись)
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
        }

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Tasks), new { lessonId });
    }

    // =========================
    // TaskDetails -> redirect Quiz/Practice
    // =========================
    public async Task<IActionResult> TaskDetails(Guid taskId)
    {
        var userId = _userManager.GetUserId(User);

        var task = await _db.LessonTasks
            .Where(t => t.Id == taskId && t.IsPublished)
            .Select(t => new
            {
                t.Id,
                t.LessonId,
                t.Title,
                t.Description,
                t.Type,
                CourseId = t.Lesson.CourseId
            })
            .FirstOrDefaultAsync();

        if (task == null) return NotFound();

        var allowed = await _db.CourseStudents.AnyAsync(cs =>
            cs.CourseId == task.CourseId && cs.StudentUserId == userId);

        if (!allowed) return Forbid();

        if (task.Type == LessonTaskType.Quiz)
            return RedirectToAction(nameof(Quiz), new { taskId });

        if (task.Type == LessonTaskType.Practice)
            return RedirectToAction(nameof(Practice), new { taskId });

        return BadRequest("Unknown task type.");
    }

    // =========================
    // QUIZ
    // =========================
    public async Task<IActionResult> Quiz(Guid taskId)
    {
        var userId = _userManager.GetUserId(User);

        var task = await _db.LessonTasks
            .Where(t => t.Id == taskId && t.IsPublished)
            .Select(t => new { t.Id, t.Title, t.LessonId, CourseId = t.Lesson.CourseId })
            .FirstOrDefaultAsync();

        if (task == null) return NotFound();

        var allowed = await _db.CourseStudents.AnyAsync(cs =>
            cs.CourseId == task.CourseId && cs.StudentUserId == userId);

        if (!allowed) return Forbid();

        var questions = await _db.QuizQuestions
            .Where(q => q.TaskId == taskId && q.IsPublished)
            .OrderBy(q => q.Order)
            .Select(q => new StudentQuizQuestionVm
            {
                QuestionId = q.Id,
                Order = q.Order,
                Text = q.Text,
                Options = q.Options
                    .OrderBy(o => o.Order)
                    .Select(o => new StudentQuizOptionVm
                    {
                        OptionId = o.Id,
                        Order = o.Order,
                        Text = o.Text
                    })
                    .ToList()
            })
            .ToListAsync();

        var attempt = await _db.StudentQuizAttempts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.TaskId == taskId && a.StudentUserId == userId);

        var vm = new StudentQuizVm
        {
            TaskId = task.Id,
            LessonId = task.LessonId,
            CourseId = task.CourseId,
            Title = task.Title,
            Questions = questions
        };

        if (attempt != null)
        {
            vm.HasResult = true;
            vm.Total = attempt.TotalQuestions;
            vm.Correct = attempt.CorrectAnswers;
            vm.ScorePercent = attempt.ScorePercent;
            vm.Passed = attempt.ScorePercent >= 70;
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitQuiz(StudentQuizSubmitVm vm)
    {
        var userId = _userManager.GetUserId(User);

        var task = await _db.LessonTasks
            .Where(t => t.Id == vm.TaskId && t.IsPublished)
            .Select(t => new { t.Id, t.LessonId, CourseId = t.Lesson.CourseId })
            .FirstOrDefaultAsync();

        if (task == null) return NotFound();

        var allowed = await _db.CourseStudents.AnyAsync(cs =>
            cs.CourseId == task.CourseId && cs.StudentUserId == userId);

        if (!allowed) return Forbid();

        var correctMap = await _db.QuizOptions
            .Where(o => o.Question.TaskId == vm.TaskId && o.Question.IsPublished && o.IsCorrect)
            .Select(o => new { o.QuestionId, o.Id })
            .ToDictionaryAsync(x => x.QuestionId, x => x.Id);

        var total = correctMap.Count;
        var correct = 0;

        foreach (var (questionId, correctOptionId) in correctMap)
        {
            if (vm.Answers.TryGetValue(questionId, out var selected) && selected == correctOptionId)
                correct++;
        }

        var score = total == 0 ? 0 : (correct * 100) / total;

        var attempt = await _db.StudentQuizAttempts
            .FirstOrDefaultAsync(a => a.TaskId == vm.TaskId && a.StudentUserId == userId);

        if (attempt == null)
        {
            attempt = new StudentQuizAttempt
            {
                Id = Guid.NewGuid(),
                TaskId = vm.TaskId,
                StudentUserId = userId!
            };
            _db.StudentQuizAttempts.Add(attempt);
        }

        attempt.TotalQuestions = total;
        attempt.CorrectAnswers = correct;
        attempt.ScorePercent = score;
        attempt.SubmittedAt = DateTime.UtcNow;

        if (score >= 70)
        {
            var progress = await _db.StudentTaskProgresses
                .FirstOrDefaultAsync(p => p.TaskId == vm.TaskId && p.StudentUserId == userId);

            if (progress == null)
            {
                progress = new StudentTaskProgress
                {
                    Id = Guid.NewGuid(),
                    TaskId = vm.TaskId,
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
            }
        }

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Quiz), new { taskId = vm.TaskId });
    }

    // =========================
    // PRACTICE (✅ FIXED)
    // =========================
    public async Task<IActionResult> Practice(Guid taskId)
    {
        var userId = _userManager.GetUserId(User);


        var taskInfo = await _db.LessonTasks
            .Where(t => t.Id == taskId && t.IsPublished)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.Description,
                t.Type,
                t.LessonId,
                CourseId = t.Lesson.CourseId
            })
            .FirstOrDefaultAsync();

        if (taskInfo == null) return NotFound();
        if (taskInfo.Type != LessonTaskType.Practice)
            return BadRequest("Task is not Practice.");

        var allowed = await _db.CourseStudents.AnyAsync(cs =>
            cs.CourseId == taskInfo.CourseId &&
            cs.StudentUserId == userId);

        if (!allowed) return Forbid();

        // ===== Practice task =====
        var practice = await _db.PracticeTasks
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.LessonTaskId == taskId);

        var starterHtml = practice?.StarterHtml ?? "<!-- write your HTML here -->";
        var starterCss = practice?.StarterCss ?? "/* write your CSS here */";
        var starterJs = practice?.StarterJs ?? "// write your JS here";

        // ===== Reference =====
        var referenceHtml = practice?.ReferenceHtml;
        var referenceCss = practice?.ReferenceCss;
        var referenceJs = practice?.ReferenceJs;

        // ===== Threshold =====
        var threshold = practice?.SimilarityThreshold ?? 80;

        // ===== Draft =====
        var draft = await _db.StudentPracticeDrafts
            .AsNoTracking()
            .FirstOrDefaultAsync(d =>
                d.LessonTaskId == taskId &&
                d.StudentUserId == userId);

        // ===== ViewModel (ОДИН РАЗ) =====
        var vm = new StudentPracticeVm
        {
            TaskId = taskId,
            LessonId = taskInfo.LessonId,
            CourseId = taskInfo.CourseId,

            Title = taskInfo.Title,
            Description = taskInfo.Description ?? "",

            // якщо є draft — показуємо його, інакше starter
            StarterHtml = draft?.Html ?? starterHtml,
            StarterCss = draft?.Css ?? starterCss,
            StarterJs = draft?.Js ?? starterJs,

            ReferenceHtml = referenceHtml,
            ReferenceCss = referenceCss,
            ReferenceJs = referenceJs,

            SimilarityThreshold = threshold,

            HasDraft = draft != null,
            DraftUpdatedAt = draft?.UpdatedAt
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitPractice(StudentPracticeSubmitVm vm)
    {
        var userId = _userManager.GetUserId(User);

        // task + access
        var taskInfo = await _db.LessonTasks
            .Where(t => t.Id == vm.TaskId && t.IsPublished)
            .Select(t => new { t.Id, t.LessonId, CourseId = t.Lesson.CourseId, t.Type })
            .FirstOrDefaultAsync();

        if (taskInfo == null) return NotFound();
        if (taskInfo.Type != LessonTaskType.Practice) return BadRequest("Not practice task.");

        var allowed = await _db.CourseStudents.AnyAsync(cs =>
            cs.CourseId == taskInfo.CourseId && cs.StudentUserId == userId);

        if (!allowed) return Forbid();

        var practice = await _db.PracticeTasks
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.LessonTaskId == vm.TaskId);

        if (practice == null)
        {
            TempData["Error"] = "Practice task is not configured yet.";
            return RedirectToAction(nameof(Practice), new { taskId = vm.TaskId });
        }

        // ✅ серверна перевірка similarity
        int Calc(string refText, string curText)
        {
            string Normalize(string s) =>
                (s ?? "").Replace("\r\n", "\n").Trim().ToLowerInvariant();

            var refTokens = System.Text.RegularExpressions.Regex
                .Matches(Normalize(refText), @"[a-z0-9_\-#\.]+")
                .Select(m => m.Value)
                .ToHashSet();

            if (refTokens.Count == 0) return -1; // не враховуємо

            var curTokens = System.Text.RegularExpressions.Regex
                .Matches(Normalize(curText), @"[a-z0-9_\-#\.]+")
                .Select(m => m.Value)
                .ToHashSet();

            var hit = refTokens.Count(t => curTokens.Contains(t));
            return (int)Math.Round(hit * 100.0 / refTokens.Count);
        }

        var parts = new List<int>();

        var pHtml = Calc(practice.ReferenceHtml, vm.Html);
        if (pHtml >= 0) parts.Add(pHtml);

        var pCss = Calc(practice.ReferenceCss, vm.Css);
        if (pCss >= 0) parts.Add(pCss);

        var pJs = Calc(practice.ReferenceJs, vm.Js);
        if (pJs >= 0) parts.Add(pJs);

        var avg = parts.Count == 0 ? 0 : (int)Math.Round(parts.Average());

        if (avg < practice.SimilarityThreshold)
        {
            TempData["Error"] = $"Not enough similarity: {avg}% (need {practice.SimilarityThreshold}%).";
            return RedirectToAction(nameof(Practice), new { taskId = vm.TaskId });
        }

        // ✅ mark completed
        var progress = await _db.StudentTaskProgresses
            .FirstOrDefaultAsync(p => p.TaskId == vm.TaskId && p.StudentUserId == userId);

        if (progress == null)
        {
            progress = new StudentTaskProgress
            {
                Id = Guid.NewGuid(),
                TaskId = vm.TaskId,
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
        }

        await _db.SaveChangesAsync();

        TempData["Success"] = $"Submitted ✅ Similarity: {avg}%";
        return RedirectToAction(nameof(Tasks), new { lessonId = taskInfo.LessonId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SavePracticeDraft([FromBody] StudentPracticeDraftSaveVm vm)
    {
        var userId = _userManager.GetUserId(User);

        // доступ + тип задачі
        var taskInfo = await _db.LessonTasks
            .Where(t => t.Id == vm.TaskId && t.IsPublished)
            .Select(t => new { t.Id, t.Type, CourseId = t.Lesson.CourseId })
            .FirstOrDefaultAsync();

        if (taskInfo == null) return NotFound();
        if (taskInfo.Type != LessonTaskType.Practice) return BadRequest();

        var allowed = await _db.CourseStudents.AnyAsync(cs =>
            cs.CourseId == taskInfo.CourseId && cs.StudentUserId == userId);

        if (!allowed) return Forbid();

        var draft = await _db.StudentPracticeDrafts
            .FirstOrDefaultAsync(d => d.LessonTaskId == vm.TaskId && d.StudentUserId == userId);

        if (draft == null)
        {
            draft = new Domain.Entities.StudentPracticeDraft
            {
                Id = Guid.NewGuid(),
                LessonTaskId = vm.TaskId,
                StudentUserId = userId!,
            };
            _db.StudentPracticeDrafts.Add(draft);
        }

        draft.Html = vm.Html ?? "";
        draft.Css = vm.Css ?? "";
        draft.Js = vm.Js ?? "";
        draft.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Json(new { ok = true, updatedAt = draft.UpdatedAt });
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