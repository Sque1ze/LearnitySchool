namespace LearnitySchool.Domain.Entities;

public class StudentTaskProgress
{
    public Guid Id { get; set; }

    public Guid TaskId { get; set; }
    public LessonTask Task { get; set; } = null!;

    // ❗ ТІЛЬКИ ID
    public string StudentUserId { get; set; } = "";

    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
}
