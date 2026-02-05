namespace LearnitySchool.Web.ViewModels.Manager.Quiz;

public class QuizQuestionEditVm
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }

    public int Order { get; set; }
    public string Text { get; set; } = "";

    public bool IsPublished { get; set; } = true;
}
