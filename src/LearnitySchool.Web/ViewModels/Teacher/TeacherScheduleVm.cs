namespace LearnitySchool.Web.ViewModels.Teacher;

public class TeacherScheduleVm
{
    public string? GroupQ { get; set; }
    public string? LessonQ { get; set; }
    public int? StudentsCount { get; set; } 

    public List<RowVm> Items { get; set; } = new();

    public class RowVm
    {
        public Guid CourseId { get; set; }
        public string CourseTitle { get; set; } = "";
        public int StudentsCount { get; set; }

        public Guid LessonId { get; set; }
        public int LessonOrder { get; set; }
        public string LessonTitle { get; set; } = "";

        public bool IsOpen { get; set; } 
    }
}
