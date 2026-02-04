namespace LearnitySchool.Domain.Entities;

public class CourseSchedule
{
    public Guid Id { get; set; }

    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;

    // День тижня (Monday, Tuesday, ...)
    public DayOfWeek DayOfWeek { get; set; }

    // Час початку заняття
    public TimeSpan StartTime { get; set; }

    // Тривалість (наприклад 01:30)
    public TimeSpan Duration { get; set; }
}
