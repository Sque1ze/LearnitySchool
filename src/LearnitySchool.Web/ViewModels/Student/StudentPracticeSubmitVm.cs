namespace LearnitySchool.Web.ViewModels.Student;

public class StudentPracticeSubmitVm
{
    public Guid TaskId { get; set; }
    public Guid LessonId { get; set; }
    public string Html { get; set; } = string.Empty;
    public string Css { get; set; } = string.Empty;
    public string Js { get; set; } = string.Empty;
    public int ClientSimilarity { get; set; }
}
