using System.ComponentModel.DataAnnotations;

namespace LearnitySchool.Web.ViewModels.Manager.Courses;

public class CourseEditVm
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Назва курсу обов’язкова.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Назва курсу має містити від 2 до 200 символів.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(4000, ErrorMessage = "Опис занадто довгий.")]
    public string? Description { get; set; }

    public bool IsPublished { get; set; }
}
