using System.ComponentModel.DataAnnotations;

namespace LearnitySchool.Web.ViewModels.Manager.Quiz;

public class QuizQuestionEditVm
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }

    [Range(1, 999, ErrorMessage = "Порядок має бути від 1 до 999.")]
    public int Order { get; set; }

    [Required(ErrorMessage = "Текст питання обов’язковий.")]
    [StringLength(4000, MinimumLength = 2)]
    public string Text { get; set; } = "";

    public bool IsPublished { get; set; } = true;
}
