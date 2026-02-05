namespace LearnitySchool.Web.ViewModels.Student;

public class StudentQuizSubmitVm
{
    public Guid TaskId { get; set; }
    public Guid LessonId { get; set; }

    // QuestionId -> SelectedOptionId
    public Dictionary<Guid, Guid> Answers { get; set; } = new();
}
