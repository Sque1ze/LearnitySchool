namespace LearnitySchool.Web.ViewModels.Teacher.Groups;

public class TeacherGroupDetailsVm
{
    public Guid CourseId { get; set; }

    public string Title { get; set; } = "";
    public string Description { get; set; } = "";

    public bool IsPublished { get; set; }
    public string Format { get; set; } = "Онлайн";

    public int StudentsCount { get; set; }

    public string TeacherName { get; set; } = "—";
    public string? ManagerName { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public string ScheduleText { get; set; } = "—";

    public List<TeacherGroupStudentRowVm> Students { get; set; } = new();



    public class SuccessLessonVm
    {
        public Guid LessonId { get; set; }
        public string Title { get; set; } = "";
        public string DateText { get; set; } = "";   // "пн, 10.11.2025, 19:00" (або просто "")
        public int Percent { get; set; }
        public bool IsSelected { get; set; }
    }

    public class SuccessStudentVm
    {
        public Guid LessonId { get; set; }           // важливо для фільтра справа
        public string StudentName { get; set; } = "";
        public string Initials { get; set; } = "A";
        public string SubText { get; set; } = "";    // optional
        public int Percent { get; set; }
    }
    public List<SuccessLessonVm> SuccessLessons { get; set; } = new();
    public List<SuccessStudentVm> SuccessStudents { get; set; } = new();

}
