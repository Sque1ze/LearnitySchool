namespace LearnitySchool.Web.ViewModels.Manager.Quiz;

public class QuizOptionListItemVm
{
    public Guid Id { get; set; }
    public int Order { get; set; }
    public string Text { get; set; } = "";
    public bool IsCorrect { get; set; }
}
