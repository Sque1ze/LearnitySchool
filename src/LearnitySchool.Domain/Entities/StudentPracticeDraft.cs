namespace LearnitySchool.Domain.Entities;

public class StudentPracticeDraft
{
    public Guid Id { get; set; }

    public Guid LessonTaskId { get; set; }           
    public LessonTask LessonTask { get; set; } = null!;

    public string StudentUserId { get; set; } = "";

    public string Html { get; set; } = "";
    public string Css { get; set; } = "";
    public string Js { get; set; } = "";

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
