using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LearnitySchool.Web.ViewModels.Manager.Courses;

public class CourseCloneVm
{
    public Guid SourceCourseId { get; set; }
    public string SourceCourseTitle { get; set; } = string.Empty;

    [Required(ErrorMessage = "Назва нового курсу обов’язкова.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Назва курсу має містити від 2 до 200 символів.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(4000, ErrorMessage = "Опис занадто довгий.")]
    public string? Description { get; set; }

    public bool IsPublished { get; set; }

    public int LessonsCount { get; set; }
    public int TasksCount { get; set; }

    public string? TeacherUserId { get; set; }
    public List<SelectListItem> Teachers { get; set; } = new();

    public List<string> SelectedStudentIds { get; set; } = new();
    public List<SelectListItem> Students { get; set; } = new();
}
