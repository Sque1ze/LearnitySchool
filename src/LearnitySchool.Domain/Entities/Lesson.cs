namespace LearnitySchool.Domain.Entities;

public class Lesson
{
    public Guid Id { get; set; }

    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>
    /// Порядок уроку в курсі (1..10)
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Контент уроку (markdown / текст / посилання)
    /// </summary>
    public string? Content { get; set; }

    public bool IsPublished { get; set; }

    // (наступним кроком тут будуть Tasks)
    public ICollection<LessonTask> Tasks { get; set; } = new List<LessonTask>();

}
