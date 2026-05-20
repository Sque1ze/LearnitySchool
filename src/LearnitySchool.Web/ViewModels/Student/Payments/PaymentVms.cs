using LearnitySchool.Domain.Enums;

namespace LearnitySchool.Web.ViewModels.Student.Payments;

public class StudentPaymentsVm
{
    public List<StudentPaymentLessonVm> Lessons { get; set; } = new();
    public List<StudentPaymentHistoryVm> History { get; set; } = new();
}

public class StudentPaymentLessonVm
{
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public Guid LessonId { get; set; }
    public int LessonOrder { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
    public decimal PriceAmount { get; set; }
    public string Currency { get; set; } = "UAH";
    public bool IsFree => PriceAmount <= 0;
    public bool IsPaid { get; set; }
    public DateTime? PaidAtUtc { get; set; }
}

public class StudentPaymentHistoryVm
{
    public Guid PaymentId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "UAH";
    public PaymentStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? PaidAtUtc { get; set; }
}

public class PaymentConfirmVm
{
    public Guid LessonId { get; set; }
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string LessonTitle { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "UAH";
}
