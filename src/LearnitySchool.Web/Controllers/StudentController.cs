using System.Text.Json;
using LearnitySchool.Application.Common;
using LearnitySchool.Domain.Entities;
using LearnitySchool.Domain.Enums;
using LearnitySchool.Infrastructure.Identity;
using LearnitySchool.Infrastructure.Persistence;
using LearnitySchool.Web.ViewModels.Student;
using LearnitySchool.Web.ViewModels.Student.Payments;
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

    private sealed class QuizAnswerSnapshot
    {
        public Dictionary<Guid, Guid> Answers { get; set; } = new();
        public Dictionary<Guid, List<Guid>> StandardAnswers { get; set; } = new();
        public Dictionary<Guid, Guid> MatchingAnswers { get; set; } = new();
        public Dictionary<Guid, string> BlankAnswers { get; set; } = new();
    }

    private static QuizAnswerSnapshot ParseQuizAnswers(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new QuizAnswerSnapshot();

        try
        {
            return JsonSerializer.Deserialize<QuizAnswerSnapshot>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new QuizAnswerSnapshot();
        }
        catch
        {
            return new QuizAnswerSnapshot();
        }
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return RedirectToAction("Login", "Account");

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
                TotalTasks = _db.LessonTasks.Count(t => t.IsPublished && t.Lesson.IsPublished && t.Lesson.CourseId == c.Id),
                CompletedTasks = _db.StudentTaskProgresses.Count(p => p.StudentUserId == userId && p.IsCompleted && p.Task.IsPublished && p.Task.Lesson.IsPublished && p.Task.Lesson.CourseId == c.Id),
                PendingReviewCount = _db.StudentTaskSubmissions.Count(s => s.StudentUserId == userId && s.Status == StudentSubmissionStatus.PendingReview && s.LessonTask.Lesson.CourseId == c.Id),
                PaidLessonsCount = _db.LessonPayments.Count(p => p.StudentId == userId && p.Status == PaymentStatus.Paid && p.Lesson.CourseId == c.Id),
                UnpaidLessonsCount = _db.Lessons.Count(l => l.CourseId == c.Id && l.IsPublished && l.PriceAmount > 0) - _db.LessonPayments.Count(p => p.StudentId == userId && p.Status == PaymentStatus.Paid && p.Lesson.CourseId == c.Id)
            })
            .ToListAsync();

        foreach (var c in courses)
        {
            c.ProgressPercent = c.TotalTasks == 0 ? 0 : (int)Math.Round((double)c.CompletedTasks * 100 / c.TotalTasks);
            if (c.UnpaidLessonsCount < 0) c.UnpaidLessonsCount = 0;
        }

        return View(courses);
    }

    public async Task<IActionResult> Lessons(Guid courseId)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return RedirectToAction("Login", "Account");

        var allowed = await _db.CourseStudents.AnyAsync(x => x.CourseId == courseId && x.StudentUserId == userId);
        if (!allowed) return Forbid();

        var gates = await _db.LessonAccesses
            .Where(g => g.CourseId == courseId)
            .Select(g => new { g.LessonId, g.IsOpen })
            .ToDictionaryAsync(x => x.LessonId, x => x.IsOpen);

        var paidLessonIds = await _db.LessonPayments
            .Where(p => p.StudentId == userId && p.Status == PaymentStatus.Paid && p.Lesson.CourseId == courseId)
            .Select(p => p.LessonId)
            .ToListAsync();

        var paidSet = paidLessonIds.ToHashSet();

        var lessons = await _db.Lessons
            .Where(l => l.CourseId == courseId && l.IsPublished)
            .OrderBy(l => l.Order)
            .Select(l => new StudentLessonVm
            {
                LessonId = l.Id,
                Order = l.Order,
                Title = l.Title,
                PriceAmount = l.PriceAmount,
                Currency = l.Currency,
                TasksCount = _db.LessonTasks.Count(t => t.LessonId == l.Id && t.IsPublished),
                CompletedTasks = _db.StudentTaskProgresses.Count(p => p.StudentUserId == userId && p.IsCompleted && p.Task.IsPublished && p.Task.LessonId == l.Id),
                ProgressPercent = 0,
                IsOpen = false
            })
            .ToListAsync();

        foreach (var l in lessons)
        {
            l.ProgressPercent = l.TasksCount == 0 ? 0 : (int)Math.Round((double)l.CompletedTasks * 100 / l.TasksCount);
            l.IsOpen = gates.TryGetValue(l.LessonId, out var open) && open;
            l.IsPaid = l.PriceAmount <= 0 || paidSet.Contains(l.LessonId);
        }

        ViewBag.CourseId = courseId;
        return View(lessons);
    }

    public async Task<IActionResult> Tasks(Guid lessonId)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return RedirectToAction("Login", "Account");

        var access = await CheckLessonAccessAsync(lessonId, userId);
        if (!access.Exists) return NotFound();
        if (!access.Enrolled) return Forbid();
        if (!access.IsOpen)
        {
            TempData["Error"] = "Цей урок зараз закритий викладачем.";
            return RedirectToAction(nameof(Lessons), new { courseId = access.CourseId });
        }
        if (!access.IsPaid)
        {
            TempData["Error"] = "Спочатку потрібно оплатити цей урок.";
            return RedirectToAction(nameof(PaymentConfirm), new { lessonId });
        }

        var tasks = await _db.LessonTasks
            .AsNoTracking()
            .Where(t => t.LessonId == lessonId && t.IsPublished)
            .OrderBy(t => t.Order)
            .Select(t => new StudentTaskVm
            {
                TaskId = t.Id,
                Order = t.Order,
                Title = t.Title,
                Type = t.Type,
                AssessmentMode = t.AssessmentMode
            })
            .ToListAsync();

        var taskIds = tasks.Select(x => x.TaskId).ToList();
        var progressMap = await _db.StudentTaskProgresses
            .AsNoTracking()
            .Where(p => p.StudentUserId == userId && taskIds.Contains(p.TaskId))
            .ToDictionaryAsync(x => x.TaskId, x => x.IsCompleted);

        var submissions = await _db.StudentTaskSubmissions
            .AsNoTracking()
            .Where(s => s.StudentUserId == userId && taskIds.Contains(s.LessonTaskId))
            .Select(s => new { s.LessonTaskId, s.Status, s.SubmittedAtUtc, s.TeacherComment, s.ServerSimilarity })
            .ToListAsync();
        var submissionMap = submissions.ToDictionary(x => x.LessonTaskId);

        var quizAttempts = await _db.StudentQuizAttempts
            .AsNoTracking()
            .Where(a => a.StudentUserId == userId && taskIds.Contains(a.TaskId))
            .Select(a => new { a.TaskId, a.ScorePercent })
            .ToListAsync();
        var quizAttemptMap = quizAttempts.ToDictionary(x => x.TaskId);

        foreach (var task in tasks)
        {
            task.IsCompleted = progressMap.TryGetValue(task.TaskId, out var done) && done;
            if (submissionMap.TryGetValue(task.TaskId, out var sub))
            {
                task.SubmissionStatus = sub.Status;
                task.SubmittedAtUtc = sub.SubmittedAtUtc;
                task.TeacherComment = sub.TeacherComment;
            }
            if (quizAttemptMap.TryGetValue(task.TaskId, out var quizAttempt))
            {
                task.HasQuizAttempt = true;
                task.QuizScorePercent = quizAttempt.ScorePercent;
            }
        }

        ViewBag.LessonId = lessonId;
        ViewBag.CourseId = access.CourseId;
        return View(tasks);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteTask(Guid taskId, Guid lessonId)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return RedirectToAction("Login", "Account");

        var access = await CheckLessonAccessAsync(lessonId, userId);
        if (!access.Exists) return NotFound();
        if (!access.Enrolled) return Forbid();
        if (!access.IsOpen || !access.IsPaid) return RedirectToAction(nameof(Lessons), new { courseId = access.CourseId });

        var task = await _db.LessonTasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == taskId && t.LessonId == lessonId && t.IsPublished);
        if (task == null) return NotFound();

        await SetTaskProgressAsync(taskId, userId, true);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Tasks), new { lessonId });
    }

    public async Task<IActionResult> TaskDetails(Guid taskId)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return RedirectToAction("Login", "Account");

        var task = await _db.LessonTasks
            .Where(t => t.Id == taskId && t.IsPublished)
            .Select(t => new { t.Id, t.LessonId, t.Type, CourseId = t.Lesson.CourseId })
            .FirstOrDefaultAsync();

        if (task == null) return NotFound();

        var access = await CheckLessonAccessAsync(task.LessonId, userId);
        if (!access.Enrolled) return Forbid();
        if (!access.IsOpen || !access.IsPaid) return RedirectToAction(nameof(Lessons), new { courseId = task.CourseId });

        if (task.Type == LessonTaskType.Quiz) return RedirectToAction(nameof(Quiz), new { taskId });
        if (task.Type == LessonTaskType.Practice) return RedirectToAction(nameof(Practice), new { taskId });
        return BadRequest("Unknown task type.");
    }

    public async Task<IActionResult> Quiz(Guid taskId)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return RedirectToAction("Login", "Account");

        var task = await _db.LessonTasks
            .Where(t => t.Id == taskId && t.IsPublished)
            .Select(t => new { t.Id, t.Title, t.LessonId, t.QuizType, CourseId = t.Lesson.CourseId })
            .FirstOrDefaultAsync();

        if (task == null) return NotFound();

        var access = await CheckLessonAccessAsync(task.LessonId, userId);
        if (!access.Enrolled) return Forbid();
        if (!access.IsOpen || !access.IsPaid)
        {
            TempData["Error"] = access.IsPaid ? "Урок закритий викладачем." : "Спочатку потрібно оплатити цей урок.";
            return RedirectToAction(nameof(Lessons), new { courseId = task.CourseId });
        }

        var questions = await _db.QuizQuestions
            .Where(q => q.TaskId == taskId && q.IsPublished)
            .OrderBy(q => q.Order)
            .Select(q => new StudentQuizQuestionVm
            {
                QuestionId = q.Id,
                Order = q.Order,
                Text = q.Text,
                Options = q.Options.OrderBy(o => o.Order).Select(o => new StudentQuizOptionVm { OptionId = o.Id, Order = o.Order, Text = o.Text, IsCorrect = o.IsCorrect }).ToList()
            })
            .ToListAsync();

        var allOptions = questions.SelectMany(q => q.Options).GroupBy(o => o.OptionId).Select(g => g.First()).OrderBy(o => o.Text).ToList();

        var attempt = await _db.StudentQuizAttempts.AsNoTracking().FirstOrDefaultAsync(a => a.TaskId == taskId && a.StudentUserId == userId);

        var vm = new StudentQuizVm
        {
            TaskId = task.Id,
            LessonId = task.LessonId,
            CourseId = task.CourseId,
            Title = task.Title,
            QuizType = task.QuizType,
            Questions = questions,
            MatchingOptions = allOptions
        };

        if (attempt != null)
        {
            vm.HasResult = true;
            vm.Total = attempt.TotalQuestions;
            vm.Correct = attempt.CorrectAnswers;
            vm.ScorePercent = attempt.ScorePercent;
            vm.Passed = attempt.ScorePercent >= 95;

            var saved = ParseQuizAnswers(attempt.AnswersJson);

            foreach (var question in vm.Questions)
            {
                if (task.QuizType == QuizType.FillBlank)
                {
                    var correctAnswers = question.Options
                        .Where(o => o.IsCorrect || question.Options.Count == 1)
                        .Select(o => NormalizeAnswer(o.Text))
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .ToHashSet();

                    question.CorrectAnswerTexts = question.Options
                        .Where(o => o.IsCorrect || question.Options.Count == 1)
                        .Select(o => o.Text)
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Distinct()
                        .ToList();

                    saved.BlankAnswers.TryGetValue(question.QuestionId, out var blankAnswer);
                    question.BlankAnswer = blankAnswer ?? string.Empty;
                    question.IsAnswered = !string.IsNullOrWhiteSpace(question.BlankAnswer);
                    question.IsCorrect = question.IsAnswered && correctAnswers.Contains(NormalizeAnswer(question.BlankAnswer));
                }
                else if (task.QuizType == QuizType.Matching)
                {
                    var correctIds = question.Options.Where(o => o.IsCorrect).Select(o => o.OptionId).ToHashSet();
                    question.CorrectOptionIds = correctIds.ToList();
                    question.CorrectAnswerTexts = question.Options.Where(o => o.IsCorrect).Select(o => o.Text).ToList();

                    var selected = saved.MatchingAnswers.TryGetValue(question.QuestionId, out var selectedMatch)
                        ? selectedMatch
                        : Guid.Empty;

                    question.SelectedMatchingOptionId = selected == Guid.Empty ? null : selected;
                    question.SelectedOptionIds = selected == Guid.Empty ? new List<Guid>() : new List<Guid> { selected };
                    question.IsAnswered = selected != Guid.Empty;
                    question.IsCorrect = question.IsAnswered && correctIds.Contains(selected);
                }
                else
                {
                    var correctIds = question.Options.Where(o => o.IsCorrect).Select(o => o.OptionId).ToHashSet();
                    question.CorrectOptionIds = correctIds.ToList();

                    var selected = saved.StandardAnswers.TryGetValue(question.QuestionId, out var selectedMany)
                        ? selectedMany.Where(x => x != Guid.Empty).Distinct().ToList()
                        : new List<Guid>();

                    if (selected.Count == 0 && saved.Answers.TryGetValue(question.QuestionId, out var oldSelected) && oldSelected != Guid.Empty)
                    {
                        selected.Add(oldSelected);
                    }

                    var selectedSet = selected.ToHashSet();
                    question.SelectedOptionIds = selected;
                    question.IsAnswered = selected.Count > 0;
                    question.IsCorrect = question.IsAnswered && correctIds.Count > 0 && selectedSet.SetEquals(correctIds);
                }
            }
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitQuiz(StudentQuizSubmitVm vm)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return RedirectToAction("Login", "Account");

        var task = await _db.LessonTasks
            .Where(t => t.Id == vm.TaskId && t.IsPublished)
            .Select(t => new { t.Id, t.LessonId, t.QuizType, CourseId = t.Lesson.CourseId })
            .FirstOrDefaultAsync();

        if (task == null) return NotFound();

        var access = await CheckLessonAccessAsync(task.LessonId, userId);
        if (!access.Enrolled) return Forbid();
        if (!access.IsOpen || !access.IsPaid) return RedirectToAction(nameof(Lessons), new { courseId = task.CourseId });

        var questions = await _db.QuizQuestions
            .Where(q => q.TaskId == vm.TaskId && q.IsPublished)
            .Include(q => q.Options)
            .ToListAsync();

        var total = questions.Count;
        var correct = 0;

        foreach (var question in questions)
        {
            if (task.QuizType == QuizType.FillBlank)
            {
                var correctAnswers = question.Options
                    .Where(o => o.IsCorrect || question.Options.Count == 1)
                    .Select(o => NormalizeAnswer(o.Text))
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToHashSet();

                if (vm.BlankAnswers.TryGetValue(question.Id, out var answer) && correctAnswers.Contains(NormalizeAnswer(answer))) correct++;
            }
            else if (task.QuizType == QuizType.Matching)
            {
                var correctOptionIds = question.Options.Where(o => o.IsCorrect).Select(o => o.Id).ToHashSet();
                if (correctOptionIds.Count == 0) continue;

                var selected = vm.MatchingAnswers.TryGetValue(question.Id, out var matchId) ? matchId : Guid.Empty;
                if (selected != Guid.Empty && correctOptionIds.Contains(selected)) correct++;
            }
            else
            {
                var correctOptionIds = question.Options.Where(o => o.IsCorrect).Select(o => o.Id).ToHashSet();
                if (correctOptionIds.Count == 0) continue;

                var selectedSet = vm.StandardAnswers.TryGetValue(question.Id, out var selectedMany)
                    ? selectedMany.Where(x => x != Guid.Empty).ToHashSet()
                    : new HashSet<Guid>();

                // Повне співпадіння множини відповідей: студент має вибрати всі правильні і не вибрати зайві.
                if (selectedSet.SetEquals(correctOptionIds)) correct++;
            }
        }

        var score = total == 0 ? 0 : (correct * 100) / total;

        var attempt = await _db.StudentQuizAttempts.FirstOrDefaultAsync(a => a.TaskId == vm.TaskId && a.StudentUserId == userId);
        if (attempt == null)
        {
            attempt = new StudentQuizAttempt { Id = Guid.NewGuid(), TaskId = vm.TaskId, StudentUserId = userId };
            _db.StudentQuizAttempts.Add(attempt);
        }

        attempt.QuizType = task.QuizType;
        attempt.TotalQuestions = total;
        attempt.CorrectAnswers = correct;
        attempt.ScorePercent = score;
        attempt.SubmittedAt = DateTime.UtcNow;
        attempt.AnswersJson = JsonSerializer.Serialize(new { vm.Answers, vm.StandardAnswers, vm.MatchingAnswers, vm.BlankAnswers });

        await SetTaskProgressAsync(vm.TaskId, userId, score >= 95);

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Quiz), new { taskId = vm.TaskId });
    }

    public async Task<IActionResult> Practice(Guid taskId)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return RedirectToAction("Login", "Account");

        var taskInfo = await _db.LessonTasks
            .Where(t => t.Id == taskId && t.IsPublished)
            .Select(t => new { t.Id, t.Title, t.Description, t.Type, t.AssessmentMode, t.LessonId, CourseId = t.Lesson.CourseId })
            .FirstOrDefaultAsync();

        if (taskInfo == null) return NotFound();
        if (taskInfo.Type != LessonTaskType.Practice) return BadRequest("Task is not Practice.");

        var access = await CheckLessonAccessAsync(taskInfo.LessonId, userId);
        if (!access.Enrolled) return Forbid();
        if (!access.IsOpen || !access.IsPaid)
        {
            TempData["Error"] = access.IsPaid ? "Урок закритий викладачем." : "Спочатку потрібно оплатити цей урок.";
            return RedirectToAction(nameof(Lessons), new { courseId = taskInfo.CourseId });
        }

        var practice = await _db.PracticeTasks.AsNoTracking().FirstOrDefaultAsync(p => p.LessonTaskId == taskId);

        var starterHtml = practice?.StarterHtml ?? string.Empty;
        var starterCss = practice?.StarterCss ?? string.Empty;
        var starterJs = practice?.StarterJs ?? string.Empty;

        var draft = await _db.StudentPracticeDrafts.AsNoTracking().FirstOrDefaultAsync(d => d.LessonTaskId == taskId && d.StudentUserId == userId);
        var submission = await _db.StudentTaskSubmissions.AsNoTracking().FirstOrDefaultAsync(s => s.LessonTaskId == taskId && s.StudentUserId == userId);

        var vm = new StudentPracticeVm
        {
            TaskId = taskId,
            LessonId = taskInfo.LessonId,
            CourseId = taskInfo.CourseId,
            Title = taskInfo.Title,
            Description = taskInfo.Description ?? string.Empty,
            Statement = practice?.Statement ?? string.Empty,
            AssessmentMode = taskInfo.AssessmentMode,
            SubmissionStatus = submission?.Status,
            SubmittedAtUtc = submission?.SubmittedAtUtc,
            TeacherComment = submission?.TeacherComment,
            StarterHtml = draft?.Html ?? submission?.Html ?? starterHtml,
            StarterCss = draft?.Css ?? submission?.Css ?? starterCss,
            StarterJs = draft?.Js ?? submission?.Js ?? starterJs,
            InitialHtml = starterHtml,
            InitialCss = starterCss,
            InitialJs = starterJs,
            ReferenceHtml = practice?.ReferenceHtml,
            ReferenceCss = practice?.ReferenceCss,
            ReferenceJs = practice?.ReferenceJs,
            SimilarityThreshold = Math.Max(practice?.SimilarityThreshold ?? 95, 95),
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
        if (string.IsNullOrWhiteSpace(userId)) return RedirectToAction("Login", "Account");

        var taskInfo = await _db.LessonTasks
            .Where(t => t.Id == vm.TaskId && t.IsPublished)
            .Select(t => new { t.Id, t.LessonId, CourseId = t.Lesson.CourseId, t.Type, t.AssessmentMode })
            .FirstOrDefaultAsync();

        if (taskInfo == null) return NotFound();
        if (taskInfo.Type != LessonTaskType.Practice) return BadRequest("Not practice task.");

        var access = await CheckLessonAccessAsync(taskInfo.LessonId, userId);
        if (!access.Enrolled) return Forbid();
        if (!access.IsOpen || !access.IsPaid) return RedirectToAction(nameof(Lessons), new { courseId = taskInfo.CourseId });

        var practice = await _db.PracticeTasks.AsNoTracking().FirstOrDefaultAsync(p => p.LessonTaskId == vm.TaskId);
        if (practice == null)
        {
            TempData["Error"] = "Practice task is not configured yet.";
            return RedirectToAction(nameof(Practice), new { taskId = vm.TaskId });
        }

        var avg = CalculateSimilarityAverage(practice, vm.Html, vm.Css, vm.Js);

        if (taskInfo.AssessmentMode == TaskAssessmentMode.TeacherReview)
        {
            var submission = await _db.StudentTaskSubmissions.FirstOrDefaultAsync(s => s.LessonTaskId == vm.TaskId && s.StudentUserId == userId);
            if (submission == null)
            {
                submission = new StudentTaskSubmission { Id = Guid.NewGuid(), LessonTaskId = vm.TaskId, StudentUserId = userId };
                _db.StudentTaskSubmissions.Add(submission);
            }

            submission.Html = vm.Html ?? string.Empty;
            submission.Css = vm.Css ?? string.Empty;
            submission.Js = vm.Js ?? string.Empty;
            submission.ClientSimilarity = vm.ClientSimilarity;
            submission.ServerSimilarity = avg;
            submission.Status = StudentSubmissionStatus.PendingReview;
            submission.SubmittedAtUtc = DateTime.UtcNow;
            submission.ReviewedAtUtc = null;
            submission.ReviewedByTeacherUserId = null;
            submission.TeacherComment = null;

            await SetTaskProgressAsync(vm.TaskId, userId, false);
            await NotifyCourseTeachersAsync(taskInfo.CourseId, "Нова робота на перевірку", "Учень здав Practice-завдання на ручну перевірку.", NotificationType.Submission, Url.Action("Details", "TeacherSubmissions", new { id = submission.Id }));
            await _db.SaveChangesAsync();

            TempData["Success"] = "Роботу здано на перевірку 🟡 Вона буде жовтою, поки вчитель її не перевірить.";
            return RedirectToAction(nameof(Tasks), new { lessonId = taskInfo.LessonId });
        }

        var autoSubmission = await _db.StudentTaskSubmissions
            .FirstOrDefaultAsync(s => s.LessonTaskId == vm.TaskId && s.StudentUserId == userId);

        if (autoSubmission == null)
        {
            autoSubmission = new StudentTaskSubmission
            {
                Id = Guid.NewGuid(),
                LessonTaskId = vm.TaskId,
                StudentUserId = userId
            };
            _db.StudentTaskSubmissions.Add(autoSubmission);
        }

        autoSubmission.Html = vm.Html ?? string.Empty;
        autoSubmission.Css = vm.Css ?? string.Empty;
        autoSubmission.Js = vm.Js ?? string.Empty;
        autoSubmission.ClientSimilarity = vm.ClientSimilarity;
        autoSubmission.ServerSimilarity = avg;
        autoSubmission.SubmittedAtUtc = DateTime.UtcNow;
        autoSubmission.ReviewedAtUtc = null;
        autoSubmission.ReviewedByTeacherUserId = null;
        autoSubmission.TeacherComment = null;

        var requiredSimilarity = Math.Max(practice.SimilarityThreshold, 95);
        var passed = avg >= requiredSimilarity;
        autoSubmission.Status = passed ? StudentSubmissionStatus.Approved : StudentSubmissionStatus.Returned;

        await SetTaskProgressAsync(vm.TaskId, userId, passed);
        await _db.SaveChangesAsync();

        TempData[passed ? "Success" : "Error"] = passed
            ? $"Submitted ✅ Similarity: {avg}%"
            : $"Роботу здано, але вона поки не зарахована: {avg}% із потрібних {requiredSimilarity}%. Можна відкрити завдання і переробити.";

        return RedirectToAction(nameof(Tasks), new { lessonId = taskInfo.LessonId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SavePracticeDraft([FromBody] StudentPracticeDraftSaveVm vm)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var taskInfo = await _db.LessonTasks
            .Where(t => t.Id == vm.TaskId && t.IsPublished)
            .Select(t => new { t.Id, t.LessonId, t.Type, CourseId = t.Lesson.CourseId })
            .FirstOrDefaultAsync();

        if (taskInfo == null) return NotFound();
        if (taskInfo.Type != LessonTaskType.Practice) return BadRequest();

        var access = await CheckLessonAccessAsync(taskInfo.LessonId, userId);
        if (!access.Enrolled) return Forbid();
        if (!access.IsOpen || !access.IsPaid) return Forbid();

        var draft = await _db.StudentPracticeDrafts.FirstOrDefaultAsync(d => d.LessonTaskId == vm.TaskId && d.StudentUserId == userId);
        if (draft == null)
        {
            draft = new StudentPracticeDraft { Id = Guid.NewGuid(), LessonTaskId = vm.TaskId, StudentUserId = userId };
            _db.StudentPracticeDrafts.Add(draft);
        }

        draft.Html = vm.Html ?? "";
        draft.Css = vm.Css ?? "";
        draft.Js = vm.Js ?? "";
        draft.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Json(new { ok = true, updatedAt = draft.UpdatedAt });
    }

    [HttpGet]
    public async Task<IActionResult> Payments()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return RedirectToAction("Login", "Account");

        var lessons = await _db.CourseStudents
            .Where(cs => cs.StudentUserId == userId)
            .SelectMany(cs => cs.Course.Lessons.Where(l => l.IsPublished).Select(l => new StudentPaymentLessonVm
            {
                CourseId = cs.CourseId,
                CourseTitle = cs.Course.Title,
                LessonId = l.Id,
                LessonOrder = l.Order,
                LessonTitle = l.Title,
                PriceAmount = l.PriceAmount,
                Currency = l.Currency
            }))
            .OrderBy(x => x.CourseTitle)
            .ThenBy(x => x.LessonOrder)
            .ToListAsync();

        var lessonIds = lessons.Select(x => x.LessonId).ToList();
        var paid = await _db.LessonPayments
            .AsNoTracking()
            .Where(p => p.StudentId == userId && lessonIds.Contains(p.LessonId) && p.Status == PaymentStatus.Paid)
            .GroupBy(p => p.LessonId)
            .Select(g => new { LessonId = g.Key, PaidAtUtc = g.Max(x => x.PaidAtUtc) })
            .ToListAsync();
        var paidMap = paid.ToDictionary(x => x.LessonId, x => x.PaidAtUtc);

        foreach (var lesson in lessons)
        {
            lesson.IsPaid = lesson.PriceAmount <= 0 || paidMap.ContainsKey(lesson.LessonId);
            lesson.PaidAtUtc = paidMap.TryGetValue(lesson.LessonId, out var paidAt) ? paidAt : null;
        }

        var history = await _db.LessonPayments
            .AsNoTracking()
            .Where(p => p.StudentId == userId)
            .OrderByDescending(p => p.CreatedAtUtc)
            .Select(p => new StudentPaymentHistoryVm
            {
                PaymentId = p.Id,
                LessonTitle = p.Lesson.Title,
                CourseTitle = p.Lesson.Course.Title,
                Amount = p.Amount,
                Currency = p.Currency,
                Status = p.Status,
                CreatedAtUtc = p.CreatedAtUtc,
                PaidAtUtc = p.PaidAtUtc
            })
            .ToListAsync();

        return View(new StudentPaymentsVm { Lessons = lessons, History = history });
    }

    [HttpGet]
    public async Task<IActionResult> PaymentConfirm(Guid lessonId)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return RedirectToAction("Login", "Account");

        var lesson = await _db.Lessons
            .AsNoTracking()
            .Where(l => l.Id == lessonId && l.IsPublished)
            .Select(l => new { l.Id, l.Title, l.PriceAmount, l.Currency, l.CourseId, CourseTitle = l.Course.Title })
            .FirstOrDefaultAsync();

        if (lesson == null) return NotFound();

        var allowed = await _db.CourseStudents.AnyAsync(cs => cs.CourseId == lesson.CourseId && cs.StudentUserId == userId);
        if (!allowed) return Forbid();

        if (lesson.PriceAmount <= 0 || await IsLessonPaidAsync(lessonId, userId))
        {
            TempData["Success"] = "Урок уже доступний.";
            return RedirectToAction(nameof(Tasks), new { lessonId });
        }

        return View(new PaymentConfirmVm
        {
            LessonId = lesson.Id,
            CourseId = lesson.CourseId,
            LessonTitle = lesson.Title,
            CourseTitle = lesson.CourseTitle,
            Amount = lesson.PriceAmount,
            Currency = lesson.Currency
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmPayment(Guid lessonId)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return RedirectToAction("Login", "Account");

        var lesson = await _db.Lessons.FirstOrDefaultAsync(l => l.Id == lessonId && l.IsPublished);
        if (lesson == null) return NotFound();

        var allowed = await _db.CourseStudents.AnyAsync(cs => cs.CourseId == lesson.CourseId && cs.StudentUserId == userId);
        if (!allowed) return Forbid();

        if (lesson.PriceAmount <= 0 || await IsLessonPaidAsync(lessonId, userId))
        {
            TempData["Success"] = "Урок уже оплачений або безкоштовний.";
            return RedirectToAction(nameof(Tasks), new { lessonId });
        }

        var payment = new LessonPayment
        {
            Id = Guid.NewGuid(),
            LessonId = lesson.Id,
            StudentId = userId,
            Amount = lesson.PriceAmount,
            Currency = lesson.Currency,
            Status = PaymentStatus.Paid,
            CreatedAtUtc = DateTime.UtcNow,
            PaidAtUtc = DateTime.UtcNow,
            PaymentProvider = "Demo",
            ProviderPaymentId = $"demo-{Guid.NewGuid():N}"
        };
        _db.LessonPayments.Add(payment);

        _db.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = "Оплата успішна",
            Message = $"Урок “{lesson.Title}” оплачено. Доступ відкрито.",
            Type = NotificationType.Payment,
            Url = Url.Action(nameof(Tasks), "Student", new { lessonId = lesson.Id })
        });

        await _db.SaveChangesAsync();
        TempData["Success"] = "Demo-оплату підтверджено. Урок доступний ✅";
        return RedirectToAction(nameof(Tasks), new { lessonId });
    }

    public IActionResult Profile() => RedirectToAction("Index", "Profile");

    private async Task<(bool Exists, bool Enrolled, bool IsOpen, bool IsPaid, Guid CourseId)> CheckLessonAccessAsync(Guid lessonId, string userId)
    {
        var lesson = await _db.Lessons
            .AsNoTracking()
            .Where(l => l.Id == lessonId && l.IsPublished)
            .Select(l => new { l.Id, l.CourseId, l.PriceAmount })
            .FirstOrDefaultAsync();

        if (lesson == null) return (false, false, false, false, Guid.Empty);

        var enrolled = await _db.CourseStudents.AnyAsync(cs => cs.CourseId == lesson.CourseId && cs.StudentUserId == userId);
        var isOpen = await _db.LessonAccesses.Where(x => x.CourseId == lesson.CourseId && x.LessonId == lessonId).Select(x => x.IsOpen).FirstOrDefaultAsync();
        var isPaid = lesson.PriceAmount <= 0 || await IsLessonPaidAsync(lessonId, userId);

        return (true, enrolled, isOpen, isPaid, lesson.CourseId);
    }

    private Task<bool> IsLessonPaidAsync(Guid lessonId, string userId)
    {
        return _db.LessonPayments.AnyAsync(p => p.LessonId == lessonId && p.StudentId == userId && p.Status == PaymentStatus.Paid);
    }

    private static string NormalizeAnswer(string? value) => (value ?? string.Empty).Trim().ToLowerInvariant();

    private static int CalculateSimilarityAverage(PracticeTask practice, string? html, string? css, string? js)
    {
        static int Calc(string? refText, string? curText)
        {
            static string Normalize(string? value) => (value ?? string.Empty).Replace("\r\n", "\n").Trim().ToLowerInvariant();
            var refTokens = System.Text.RegularExpressions.Regex.Matches(Normalize(refText), @"[a-z0-9_\-#\.]+")
                .Select(m => m.Value).ToHashSet();
            if (refTokens.Count == 0) return -1;
            var curTokens = System.Text.RegularExpressions.Regex.Matches(Normalize(curText), @"[a-z0-9_\-#\.]+")
                .Select(m => m.Value).ToHashSet();
            var hit = refTokens.Count(t => curTokens.Contains(t));
            return (int)Math.Round(hit * 100.0 / refTokens.Count);
        }

        var parts = new List<int>();
        var pHtml = Calc(practice.ReferenceHtml, html); if (pHtml >= 0) parts.Add(pHtml);
        var pCss = Calc(practice.ReferenceCss, css); if (pCss >= 0) parts.Add(pCss);
        var pJs = Calc(practice.ReferenceJs, js); if (pJs >= 0) parts.Add(pJs);
        return parts.Count == 0 ? 0 : (int)Math.Round(parts.Average());
    }

    private async Task SetTaskProgressAsync(Guid taskId, string studentUserId, bool isCompleted)
    {
        var progress = await _db.StudentTaskProgresses.FirstOrDefaultAsync(p => p.TaskId == taskId && p.StudentUserId == studentUserId);
        if (progress == null)
        {
            progress = new StudentTaskProgress { Id = Guid.NewGuid(), TaskId = taskId, StudentUserId = studentUserId };
            _db.StudentTaskProgresses.Add(progress);
        }
        progress.IsCompleted = isCompleted;
        progress.CompletedAt = isCompleted ? DateTime.UtcNow : null;
    }

    private async Task NotifyCourseTeachersAsync(Guid courseId, string title, string message, NotificationType type, string? url)
    {
        var teacherIds = await _db.CourseTeachers.Where(t => t.CourseId == courseId).Select(t => t.TeacherUserId).Distinct().ToListAsync();
        foreach (var teacherId in teacherIds)
        {
            _db.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = teacherId,
                Title = title,
                Message = message,
                Type = type,
                Url = url
            });
        }
    }
}
