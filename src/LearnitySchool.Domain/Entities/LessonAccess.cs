namespace LearnitySchool.Domain.Entities;

public class LessonAccess
{
    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public Guid LessonId { get; set; }
    public Lesson Lesson { get; set; } = null!;

    public bool IsOpen { get; set; } = false;

    public DateTime? OpenedAtUtc { get; set; }
    public string? OpenedByUserId { get; set; } // TeacherId who toggled last
}
