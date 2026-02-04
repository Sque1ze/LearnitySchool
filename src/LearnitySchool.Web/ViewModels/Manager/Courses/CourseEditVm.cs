namespace LearnitySchool.Web.ViewModels.Manager.Courses;

public class CourseEditVm
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsPublished { get; set; }
}
