namespace LearnitySchool.Web.ViewModels.Manager.Quiz;

public class QuizQuestionListItemVm
{
    public Guid Id { get; set; }
    public int Order { get; set; }
    public string Text { get; set; } = "";
    public bool IsPublished { get; set; }

    public int OptionsCount { get; set; }
    public bool HasCorrectOption { get; set; }
}
