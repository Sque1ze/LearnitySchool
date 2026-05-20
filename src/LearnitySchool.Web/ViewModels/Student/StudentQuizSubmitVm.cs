namespace LearnitySchool.Web.ViewModels.Student;

public class StudentQuizSubmitVm
{
    public Guid TaskId { get; set; }
    public Guid LessonId { get; set; }

    // Старе поле залишене для сумісності з попередньою версією.
    public Dictionary<Guid, Guid> Answers { get; set; } = new();

    // Standard Quiz: підтримує одну або кілька правильних відповідей.
    public Dictionary<Guid, List<Guid>> StandardAnswers { get; set; } = new();
    public Dictionary<Guid, Guid> MatchingAnswers { get; set; } = new();
    public Dictionary<Guid, string> BlankAnswers { get; set; } = new();
}
