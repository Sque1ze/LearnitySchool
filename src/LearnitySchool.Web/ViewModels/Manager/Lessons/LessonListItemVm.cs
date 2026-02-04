namespace LearnitySchool.Web.ViewModels.Manager.Lessons;

public class LessonListItemVm
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }

    public int Order { get; set; }
    public string Title { get; set; } = string.Empty;

    public bool IsPublished { get; set; }
}
