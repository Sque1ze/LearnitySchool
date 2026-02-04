namespace LearnitySchool.Domain.Entities;

public class Course
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsPublished { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Навігація
    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    public ICollection<CourseSchedule> Schedules { get; set; } = new List<CourseSchedule>();
    public ICollection<CourseTeacher> Teachers { get; set; } = new List<CourseTeacher>();
    public ICollection<CourseStudent> Students { get; set; } = new List<CourseStudent>();

}
