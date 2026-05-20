namespace LearnitySchool.Domain.Entities;

public class PracticeTask
{
    public Guid Id { get; set; }

    // 1:1 з LessonTask
    public Guid LessonTaskId { get; set; }
    public LessonTask LessonTask { get; set; } = null!;

    // Умова
    public string Statement { get; set; } = string.Empty;

    // Starter code
    public string StarterHtml { get; set; } = "";
    public string StarterCss { get; set; } = "";
    public string StarterJs { get; set; } = "";

    // Reference solution
    public string ReferenceHtml { get; set; } = "";
    public string ReferenceCss { get; set; } = "";
    public string ReferenceJs { get; set; } = "";

    // % для зарахування
    public int SimilarityThreshold { get; set; } = 95;
}
