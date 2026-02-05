namespace LearnitySchool.Web.ViewModels.Student;

public class StudentQuizVm
{
    public Guid TaskId { get; set; }
    public Guid LessonId { get; set; }
    public Guid CourseId { get; set; }

    public string Title { get; set; } = "";

    public List<StudentQuizQuestionVm> Questions { get; set; } = new();

    // результат (після сабміту)
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
}

public class StudentQuizOptionVm
{
    public Guid OptionId { get; set; }
    public int Order { get; set; }
    public string Text { get; set; } = "";
}
