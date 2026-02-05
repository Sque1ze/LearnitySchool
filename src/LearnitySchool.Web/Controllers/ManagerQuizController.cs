using LearnitySchool.Application.Common;
using LearnitySchool.Domain.Entities;
using LearnitySchool.Infrastructure.Persistence;
using LearnitySchool.Web.ViewModels.Manager.Quiz;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearnitySchool.Web.Controllers;

[Authorize(Roles = RoleNames.Manager)]
public class ManagerQuizController : Controller
{
    private readonly AppDbContext _db;

    public ManagerQuizController(AppDbContext db)
    {
        _db = db;
    }

    // =========================
    // QUESTIONS LIST
    // /ManagerQuiz/Quiz?taskId=...
    // =========================
    public async Task<IActionResult> Quiz(Guid taskId)
    {
        var task = await _db.LessonTasks
            .AsNoTracking()
            .Where(t => t.Id == taskId)
            .Select(t => new { t.Id, t.Title, t.LessonId })
            .FirstOrDefaultAsync();

        if (task == null) return NotFound();

        var questions = await _db.QuizQuestions
            .AsNoTracking()
            .Where(q => q.TaskId == taskId)
            .OrderBy(q => q.Order)
            .Select(q => new QuizQuestionListItemVm
            {
                Id = q.Id,
                Order = q.Order,
                Text = q.Text,
                IsPublished = q.IsPublished,
                OptionsCount = q.Options.Count,
                HasCorrectOption = q.Options.Any(o => o.IsCorrect)
            })
            .ToListAsync();

        ViewBag.TaskId = taskId;
        ViewBag.TaskTitle = task.Title;
        ViewBag.LessonId = task.LessonId;

        return View(questions);
    }

    // =========================
    // CREATE QUESTION (GET)
    // =========================
    public IActionResult CreateQuestion(Guid taskId)
    {
        return View(new QuizQuestionEditVm
        {
            TaskId = taskId,
            Order = 1,
            IsPublished = true
        });
    }

    // =========================
    // CREATE QUESTION (POST)
    // =========================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateQuestion(QuizQuestionEditVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var q = new QuizQuestion
        {
            Id = Guid.NewGuid(),
            TaskId = vm.TaskId,
            Order = vm.Order,
            Text = vm.Text.Trim(),
            IsPublished = vm.IsPublished
        };

        _db.QuizQuestions.Add(q);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Quiz), new { taskId = vm.TaskId });
    }

    // =========================
    // EDIT QUESTION (GET)
    // =========================
    public async Task<IActionResult> EditQuestion(Guid id)
    {
        var q = await _db.QuizQuestions.FindAsync(id);
        if (q == null) return NotFound();

        return View(new QuizQuestionEditVm
        {
            Id = q.Id,
            TaskId = q.TaskId,
            Order = q.Order,
            Text = q.Text,
            IsPublished = q.IsPublished
        });
    }

    // =========================
    // EDIT QUESTION (POST)
    // =========================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditQuestion(QuizQuestionEditVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var q = await _db.QuizQuestions.FindAsync(vm.Id);
        if (q == null) return NotFound();

        q.Order = vm.Order;
        q.Text = vm.Text.Trim();
        q.IsPublished = vm.IsPublished;

        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Quiz), new { taskId = q.TaskId });
    }

    // =========================
    // DELETE QUESTION (GET)
    // =========================
    public async Task<IActionResult> DeleteQuestion(Guid id)
    {
        var q = await _db.QuizQuestions
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new QuizQuestionEditVm
            {
                Id = x.Id,
                TaskId = x.TaskId,
                Order = x.Order,
                Text = x.Text,
                IsPublished = x.IsPublished
            })
            .FirstOrDefaultAsync();

        if (q == null) return NotFound();
        return View(q);
    }

    // =========================
    // DELETE QUESTION (POST)
    // =========================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteQuestionConfirmed(Guid id)
    {
        var q = await _db.QuizQuestions.FindAsync(id);
        if (q == null) return NotFound();

        var taskId = q.TaskId;

        _db.QuizQuestions.Remove(q);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Quiz), new { taskId });
    }

    // =========================
    // OPTIONS LIST
    // /ManagerQuiz/Options?questionId=...
    // =========================
    public async Task<IActionResult> Options(Guid questionId)
    {
        var q = await _db.QuizQuestions
            .AsNoTracking()
            .Where(x => x.Id == questionId)
            .Select(x => new { x.Id, x.TaskId, x.Text })
            .FirstOrDefaultAsync();

        if (q == null) return NotFound();

        var options = await _db.QuizOptions
            .AsNoTracking()
            .Where(o => o.QuestionId == questionId)
            .OrderBy(o => o.Order)
            .Select(o => new QuizOptionListItemVm
            {
                Id = o.Id,
                Order = o.Order,
                Text = o.Text,
                IsCorrect = o.IsCorrect
            })
            .ToListAsync();

        ViewBag.QuestionId = questionId;
        ViewBag.TaskId = q.TaskId;
        ViewBag.QuestionText = q.Text;

        return View(options);
    }

    // =========================
    // CREATE OPTION (GET)
    // =========================
    public IActionResult CreateOption(Guid questionId)
    {
        return View(new QuizOptionEditVm
        {
            QuestionId = questionId,
            Order = 1
        });
    }

    // =========================
    // CREATE OPTION (POST)
    // =========================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateOption(QuizOptionEditVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        // якщо ставимо correct — скидаємо інші
        if (vm.IsCorrect)
        {
            var others = await _db.QuizOptions
                .Where(o => o.QuestionId == vm.QuestionId)
                .ToListAsync();

            foreach (var o in others)
                o.IsCorrect = false;
        }

        var opt = new QuizOption
        {
            Id = Guid.NewGuid(),
            QuestionId = vm.QuestionId,
            Order = vm.Order,
            Text = vm.Text.Trim(),
            IsCorrect = vm.IsCorrect
        };

        _db.QuizOptions.Add(opt);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Options), new { questionId = vm.QuestionId });
    }

    // =========================
    // EDIT OPTION (GET)
    // =========================
    public async Task<IActionResult> EditOption(Guid id)
    {
        var o = await _db.QuizOptions.FindAsync(id);
        if (o == null) return NotFound();

        return View(new QuizOptionEditVm
        {
            Id = o.Id,
            QuestionId = o.QuestionId,
            Order = o.Order,
            Text = o.Text,
            IsCorrect = o.IsCorrect
        });
    }

    // =========================
    // EDIT OPTION (POST)
    // =========================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditOption(QuizOptionEditVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var o = await _db.QuizOptions.FindAsync(vm.Id);
        if (o == null) return NotFound();

        // якщо ставимо correct — скидаємо інші
        if (vm.IsCorrect)
        {
            var others = await _db.QuizOptions
                .Where(x => x.QuestionId == vm.QuestionId && x.Id != vm.Id)
                .ToListAsync();

            foreach (var x in others)
                x.IsCorrect = false;
        }

        o.Order = vm.Order;
        o.Text = vm.Text.Trim();
        o.IsCorrect = vm.IsCorrect;

        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Options), new { questionId = vm.QuestionId });
    }

    // =========================
    // DELETE OPTION (GET)
    // =========================
    public async Task<IActionResult> DeleteOption(Guid id)
    {
        var o = await _db.QuizOptions
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new QuizOptionEditVm
            {
                Id = x.Id,
                QuestionId = x.QuestionId,
                Order = x.Order,
                Text = x.Text,
                IsCorrect = x.IsCorrect
            })
            .FirstOrDefaultAsync();

        if (o == null) return NotFound();
        return View(o);
    }

    // =========================
    // DELETE OPTION (POST)
    // =========================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteOptionConfirmed(Guid id)
    {
        var o = await _db.QuizOptions.FindAsync(id);
        if (o == null) return NotFound();

        var qId = o.QuestionId;

        _db.QuizOptions.Remove(o);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Options), new { questionId = qId });
    }
}
