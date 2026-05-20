using LearnitySchool.Domain.Enums;

namespace LearnitySchool.Domain.Entities;

public class StudentTaskSubmission
{
    public Guid Id { get; set; }

    public Guid LessonTaskId { get; set; }
    public LessonTask LessonTask { get; set; } = null!;

    public string StudentUserId { get; set; } = string.Empty;

    public string Html { get; set; } = string.Empty;
    public string Css { get; set; } = string.Empty;
    public string Js { get; set; } = string.Empty;

    public int? ClientSimilarity { get; set; }
    public int? ServerSimilarity { get; set; }

    public StudentSubmissionStatus Status { get; set; } = StudentSubmissionStatus.PendingReview;

    public DateTime SubmittedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAtUtc { get; set; }
    public string? ReviewedByTeacherUserId { get; set; }
    public string? TeacherComment { get; set; }
}
