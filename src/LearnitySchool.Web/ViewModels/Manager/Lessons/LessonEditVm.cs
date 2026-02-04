namespace LearnitySchool.Web.ViewModels.Manager.Lessons;

public class LessonEditVm
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }

    public int Order { get; set; } = 1;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Content { get; set; }

    public bool IsPublished { get; set; }

    // для заголовка на сторінці
    public string CourseTitle { get; set; } = string.Empty;
}
