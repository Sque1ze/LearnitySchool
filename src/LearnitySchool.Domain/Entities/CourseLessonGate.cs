namespace LearnitySchool.Domain.Entities;

public class CourseLessonGate
{
    public Guid Id { get; set; }

    public Guid CourseId { get; set; }
    public Guid LessonId { get; set; }

    public bool IsOpen { get; set; }
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public string UpdatedByTeacherUserId { get; set; } = string.Empty;
}
