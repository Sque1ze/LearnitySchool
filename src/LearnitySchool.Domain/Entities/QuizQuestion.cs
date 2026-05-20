namespace LearnitySchool.Domain.Entities;

public class QuizQuestion
{
    public Guid Id { get; set; }

    public Guid TaskId { get; set; }              
    public LessonTask Task { get; set; } = null!;

    public string Text { get; set; } = string.Empty;

    public int Order { get; set; }                
    public bool IsPublished { get; set; } = true;

    public List<QuizOption> Options { get; set; } = new();
}
