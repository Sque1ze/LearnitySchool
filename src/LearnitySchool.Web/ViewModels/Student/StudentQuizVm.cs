using LearnitySchool.Domain.Enums;

namespace LearnitySchool.Web.ViewModels.Student;

public class StudentQuizVm
{
    public Guid TaskId { get; set; }
    public Guid LessonId { get; set; }
    public Guid CourseId { get; set; }

    public string Title { get; set; } = "";
    public QuizType QuizType { get; set; } = QuizType.Standard;

    public List<StudentQuizQuestionVm> Questions { get; set; } = new();
    public List<StudentQuizOptionVm> MatchingOptions { get; set; } = new();

    public bool HasResult { get; set; }
    public int Total { get; set; }
    public int Correct { get; set; }
    public int ScorePercent { get; set; }
    public bool Passed { get; set; }
}

public class StudentQuizQuestionVm
{
    public Guid QuestionId { get; set; }
    public int Order { get; set; }
    public string Text { get; set; } = "";
    public List<StudentQuizOptionVm> Options { get; set; } = new();

    public bool? IsCorrect { get; set; }
    public bool IsAnswered { get; set; }
    public List<Guid> SelectedOptionIds { get; set; } = new();
    public Guid? SelectedMatchingOptionId { get; set; }
    public string? BlankAnswer { get; set; }
    public List<Guid> CorrectOptionIds { get; set; } = new();
    public List<string> CorrectAnswerTexts { get; set; } = new();
}

public class StudentQuizOptionVm
{
    public Guid OptionId { get; set; }
    public int Order { get; set; }
    public string Text { get; set; } = "";
    public bool IsCorrect { get; set; }
}
