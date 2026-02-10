namespace LearnitySchool.Web.ViewModels.Teacher.Groups;

public class TeacherStudentTasksVm
{
    public Guid CourseId { get; set; }
    public string StudentUserId { get; set; } = "";
    public string StudentName { get; set; } = "—";

    public List<LessonBlockVm> Lessons { get; set; } = new();

    public class LessonBlockVm
    {
        public Guid LessonId { get; set; }
        public int Order { get; set; }
        public string Title { get; set; } = "";

        public List<TaskSquareVm> Tasks { get; set; } = new();
    }

    public class TaskSquareVm
    {
        public Guid TaskId { get; set; }
        public string Title { get; set; } = "";
        public int Order { get; set; }
        public bool IsCompleted { get; set; }
    }
}
