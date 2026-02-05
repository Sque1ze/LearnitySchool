namespace LearnitySchool.Web.ViewModels.Manager.Quiz;

public class QuizOptionEditVm
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }

    public int Order { get; set; }
    public string Text { get; set; } = "";

    public bool IsCorrect { get; set; }
}
