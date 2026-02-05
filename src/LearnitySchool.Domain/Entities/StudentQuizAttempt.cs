namespace LearnitySchool.Domain.Entities;

public class StudentQuizAttempt
{
    public Guid Id { get; set; }

    public Guid TaskId { get; set; }             
    public string StudentUserId { get; set; } = "";

    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public int ScorePercent { get; set; }

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}
