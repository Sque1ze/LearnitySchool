namespace LearnitySchool.Web.ViewModels.Teacher.Groups;

public class TeacherLessonAccessRowVm
{
    public Guid LessonId { get; set; }
    public int Order { get; set; }
    public string Title { get; set; } = "";
    public bool IsOpen { get; set; }
    public int TasksCount { get; set; }
}
