using Microsoft.AspNetCore.Mvc.Rendering;

namespace LearnitySchool.Web.ViewModels.Manager.Courses;

public class CoursePeopleVm
{
    public Guid CourseId { get; set; }

    public string CourseTitle { get; set; } = string.Empty;

    // 1 teacher (на старті)
    public string? TeacherUserId { get; set; }
    public List<SelectListItem> Teachers { get; set; } = new();

    // many students
    public List<string> SelectedStudentIds { get; set; } = new();
    public List<SelectListItem> Students { get; set; } = new();
}
