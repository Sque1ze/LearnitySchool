public class StudentPracticeSubmitVm
{
    public Guid TaskId { get; set; }
    public Guid LessonId { get; set; }
    public string Html { get; set; } = "";
    public string Css { get; set; } = "";
    public string Js { get; set; } = "";
    public int ClientSimilarity { get; set; } 
}
