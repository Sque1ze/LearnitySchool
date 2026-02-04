using LearnitySchool.Domain.Enums;

namespace LearnitySchool.Domain.Entities;

public class LessonTask
{
    public Guid Id { get; set; }

    public Guid LessonId { get; set; }
    public Lesson Lesson { get; set; } = null!;

    public int Order { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public LessonTaskType Type { get; set; }

    public bool IsPublished { get; set; }
}
