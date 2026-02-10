namespace LearnitySchool.Web.ViewModels.Student;

public class StudentLessonVm
{
    public Guid LessonId { get; set; }
    public int Order { get; set; }
    public string Title { get; set; } = "";

    public int TasksCount { get; set; }
    public int CompletedTasks { get; set; }
    public int ProgressPercent { get; set; }
    public bool IsOpen { get; set; }
}
