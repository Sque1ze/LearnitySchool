using LearnitySchool.Domain.Enums;

namespace LearnitySchool.Web.ViewModels.Student;

public class StudentTaskVm
{
    public Guid TaskId { get; set; }
    public int Order { get; set; }
    public string Title { get; set; } = "";
    public LessonTaskType Type { get; set; }
    public bool IsCompleted { get; set; }
}
