namespace LearnitySchool.Web.ViewModels.Student;

public class StudentLessonVm
{
    public Guid LessonId { get; set; }
    public int Order { get; set; }
    public string Title { get; set; } = "";

    public int TasksCount { get; set; }
    public int CompletedTasks { get; set; }
    public int ProgressPercent { get; set; }
    public bool IsOpen { get; set; }

    public decimal PriceAmount { get; set; }
    public string Currency { get; set; } = "UAH";
    public bool IsFree => PriceAmount <= 0;
    public bool IsPaid { get; set; }
    public bool RequiresPayment => PriceAmount > 0;
    public bool CanEnter => IsOpen && (IsFree || IsPaid);
}
