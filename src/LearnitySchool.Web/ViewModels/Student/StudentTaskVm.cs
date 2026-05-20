using LearnitySchool.Domain.Enums;

namespace LearnitySchool.Web.ViewModels.Student;

public class StudentTaskVm
{
    public Guid TaskId { get; set; }
    public int Order { get; set; }
    public string Title { get; set; } = string.Empty;
    public LessonTaskType Type { get; set; }
    public TaskAssessmentMode AssessmentMode { get; set; }
    public bool IsCompleted { get; set; }
    public StudentSubmissionStatus? SubmissionStatus { get; set; }
    public DateTime? SubmittedAtUtc { get; set; }
    public string? TeacherComment { get; set; }
    public bool HasQuizAttempt { get; set; }
    public int? QuizScorePercent { get; set; }
    public bool IsFailedAttempt => !IsCompleted && ((SubmissionStatus == StudentSubmissionStatus.Returned) || (HasQuizAttempt && (QuizScorePercent ?? 0) < 95));
}
