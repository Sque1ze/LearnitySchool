using LearnitySchool.Domain.Enums;

namespace LearnitySchool.Web.ViewModels.Student;

public class StudentPracticeVm
{
    public Guid TaskId { get; set; }
    public Guid LessonId { get; set; }
    public Guid CourseId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Statement { get; set; } = string.Empty;

    public TaskAssessmentMode AssessmentMode { get; set; } = TaskAssessmentMode.AutoPercent;
    public StudentSubmissionStatus? SubmissionStatus { get; set; }
    public DateTime? SubmittedAtUtc { get; set; }
    public string? TeacherComment { get; set; }

    // starter або остання чернетка
    public string StarterHtml { get; set; } = string.Empty;
    public string StarterCss { get; set; } = string.Empty;
    public string StarterJs { get; set; } = string.Empty;

    public string? ReferenceHtml { get; set; }
    public string? ReferenceCss { get; set; }
    public string? ReferenceJs { get; set; }
    public int SimilarityThreshold { get; set; } = 80;

    public bool HasReference =>
        !string.IsNullOrWhiteSpace(ReferenceHtml) ||
        !string.IsNullOrWhiteSpace(ReferenceCss) ||
        !string.IsNullOrWhiteSpace(ReferenceJs);

    public bool IsTeacherReview => AssessmentMode == TaskAssessmentMode.TeacherReview;

    public bool HasDraft { get; set; }
    public DateTime? DraftUpdatedAt { get; set; }

    // справжній starter для кнопки Reset
    public string InitialHtml { get; set; } = string.Empty;
    public string InitialCss { get; set; } = string.Empty;
    public string InitialJs { get; set; } = string.Empty;
}
