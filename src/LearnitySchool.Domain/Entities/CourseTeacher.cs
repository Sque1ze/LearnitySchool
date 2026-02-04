namespace LearnitySchool.Domain.Entities;

public class CourseTeacher
{
    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;

    // Identity User (Teacher)
    public string TeacherUserId { get; set; } = string.Empty;
}
