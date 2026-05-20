namespace LearnitySchool.Domain.Entities;

public class StudentProject
{
    public Guid Id { get; set; }

    public string StudentId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public string Html { get; set; } = string.Empty;
    public string Css { get; set; } = string.Empty;
    public string Js { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
