namespace LearnitySchool.Web.ViewModels.Student;

public class StudentCourseVm
{
    public Guid CourseId { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }

    public int LessonsCount { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int ProgressPercent { get; set; }
}
