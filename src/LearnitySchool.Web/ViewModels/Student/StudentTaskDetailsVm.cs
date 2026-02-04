namespace LearnitySchool.Web.ViewModels.Student;

public class StudentTaskDetailsVm
{
    public Guid TaskId { get; set; }
    public Guid LessonId { get; set; }
    public Guid CourseId { get; set; }

    public int Order { get; set; }
    public string Title { get; set; } = "";
    public string Type { get; set; } = ""; // або твій enum -> ToString()
    public string Description { get; set; } = "";

    public bool IsCompleted { get; set; }
}
