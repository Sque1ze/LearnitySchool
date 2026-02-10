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
        public string DateText { get; set; } = "";   
        public int Percent { get; set; }
        public bool IsSelected { get; set; }
        public bool IsOpen { get; set; }
    }

    public class SuccessStudentVm
    {
        public Guid LessonId { get; set; }           
        public string StudentName { get; set; } = "";
        public string Initials { get; set; } = "A";
        public string SubText { get; set; } = "";   
        public int Percent { get; set; }
    }
    public List<SuccessLessonVm> SuccessLessons { get; set; } = new();
    public List<SuccessStudentVm> SuccessStudents { get; set; } = new();

    public class AttendanceLessonVm
    {
        public Guid LessonId { get; set; }
        public int Order { get; set; }
        public string Title { get; set; } = "";
    }

    public class AttendanceStudentVm
    {
        public string StudentUserId { get; set; } = "";
        public string StudentName { get; set; } = "—";
        public string Initials { get; set; } = "•";
        public string? SubText { get; set; }
        public List<int> Statuses { get; set; } = new(); 
    }

    public List<AttendanceLessonVm> AttendanceLessons { get; set; } = new();
    public List<AttendanceStudentVm> AttendanceStudents { get; set; } = new();

}
