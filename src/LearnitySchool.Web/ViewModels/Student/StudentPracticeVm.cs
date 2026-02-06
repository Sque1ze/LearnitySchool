namespace LearnitySchool.Web.ViewModels.Student;

public class StudentPracticeVm
{
    public Guid TaskId { get; set; }
    public Guid LessonId { get; set; }
    public Guid CourseId { get; set; }

    public string Title { get; set; } = "";
    public string? Description { get; set; }

    // starter
    public string StarterHtml { get; set; } = "";
    public string StarterCss { get; set; } = "";
    public string StarterJs { get; set; } = "";

    public string ReferenceHtml { get; set; } = "";
    public string ReferenceCss { get; set; } = "";
    public string ReferenceJs { get; set; } = "";
}
