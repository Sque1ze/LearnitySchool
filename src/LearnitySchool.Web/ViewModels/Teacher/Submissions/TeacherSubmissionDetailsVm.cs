using LearnitySchool.Domain.Enums;

namespace LearnitySchool.Web.ViewModels.Teacher.Submissions;

public class TeacherSubmissionDetailsVm
{
    public Guid SubmissionId { get; set; }
    public Guid CourseId { get; set; }
    public Guid LessonId { get; set; }
    public Guid TaskId { get; set; }

    public string StudentUserId { get; set; } = string.Empty;
    public string StudentName { get; set; } = "—";
    public string? StudentEmail { get; set; }

    public string CourseTitle { get; set; } = string.Empty;
    public string LessonTitle { get; set; } = string.Empty;
    public string TaskTitle { get; set; } = string.Empty;
    public string? TaskDescription { get; set; }
    public string? Statement { get; set; }

    public StudentSubmissionStatus Status { get; set; }
    public int? ClientSimilarity { get; set; }
    public int? ServerSimilarity { get; set; }
    public DateTime SubmittedAtUtc { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }
    public string? TeacherComment { get; set; }

    public string Html { get; set; } = string.Empty;
    public string Css { get; set; } = string.Empty;
    public string Js { get; set; } = string.Empty;

    public string ReferenceHtml { get; set; } = string.Empty;
    public string ReferenceCss { get; set; } = string.Empty;
    public string ReferenceJs { get; set; } = string.Empty;

    public bool HasReference =>
        !string.IsNullOrWhiteSpace(ReferenceHtml) ||
        !string.IsNullOrWhiteSpace(ReferenceCss) ||
        !string.IsNullOrWhiteSpace(ReferenceJs);
}
