namespace LearnitySchool.Domain.Entities;

public class CourseStudent
{
    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;

    // Identity User (Student)
    public string StudentUserId { get; set; } = string.Empty;

    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
}
