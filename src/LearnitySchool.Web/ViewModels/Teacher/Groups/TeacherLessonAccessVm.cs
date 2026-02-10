namespace LearnitySchool.Web.ViewModels.Teacher.Groups;

public class TeacherLessonAccessVm
{
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = "";
    public List<TeacherLessonAccessRowVm> Lessons { get; set; } = new();
}
