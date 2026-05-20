using LearnitySchool.Domain.Enums;

namespace LearnitySchool.Domain.Entities;

public class LessonPayment
{
    public Guid Id { get; set; }

    public string StudentId { get; set; } = string.Empty;

    public Guid LessonId { get; set; }
    public Lesson Lesson { get; set; } = null!;

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "UAH";

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAtUtc { get; set; }

    public string PaymentProvider { get; set; } = "Demo";
    public string? ProviderPaymentId { get; set; }
}
