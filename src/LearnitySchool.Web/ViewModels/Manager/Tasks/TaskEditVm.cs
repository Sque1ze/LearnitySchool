using System.ComponentModel.DataAnnotations;
using LearnitySchool.Domain.Enums;

namespace LearnitySchool.Web.ViewModels.Manager.Tasks;

public class TaskEditVm
{
    public Guid Id { get; set; }

    public Guid LessonId { get; set; }

    [Range(1, 999, ErrorMessage = "Порядок завдання має бути від 1 до 999.")]
    public int Order { get; set; } = 1;

    [Required(ErrorMessage = "Назва завдання обов’язкова.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Назва завдання має містити від 2 до 200 символів.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(4000, ErrorMessage = "Опис занадто довгий.")]
    public string? Description { get; set; }

    public LessonTaskType Type { get; set; } = LessonTaskType.Quiz;

    public TaskAssessmentMode AssessmentMode { get; set; } = TaskAssessmentMode.AutoPercent;

    public QuizType QuizType { get; set; } = QuizType.Standard;

    public bool IsPublished { get; set; }

    public string LessonTitle { get; set; } = string.Empty;
}
