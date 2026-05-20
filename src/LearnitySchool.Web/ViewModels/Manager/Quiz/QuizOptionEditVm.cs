using System.ComponentModel.DataAnnotations;

namespace LearnitySchool.Web.ViewModels.Manager.Quiz;

public class QuizOptionEditVm
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }

    [Range(1, 999, ErrorMessage = "Порядок має бути від 1 до 999.")]
    public int Order { get; set; }

    [Required(ErrorMessage = "Текст відповіді обов’язковий.")]
    [StringLength(2000, MinimumLength = 1)]
    public string Text { get; set; } = "";

    public bool IsCorrect { get; set; }
}
