using System.ComponentModel.DataAnnotations;

namespace LearnitySchool.Web.ViewModels.Manager.Lessons;

public class LessonEditVm
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }

    [Range(1, 999, ErrorMessage = "Порядок уроку має бути від 1 до 999.")]
    public int Order { get; set; } = 1;

    [Required(ErrorMessage = "Назва уроку обов’язкова.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Назва уроку має містити від 2 до 200 символів.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(4000, ErrorMessage = "Опис занадто довгий.")]
    public string? Description { get; set; }

    public string? Content { get; set; }

    public bool IsPublished { get; set; }

    [Range(0, 100000, ErrorMessage = "Ціна має бути від 0 до 100000.")]
    public decimal PriceAmount { get; set; }

    [Required]
    [StringLength(8)]
    public string Currency { get; set; } = "UAH";

    public string CourseTitle { get; set; } = string.Empty;
}
