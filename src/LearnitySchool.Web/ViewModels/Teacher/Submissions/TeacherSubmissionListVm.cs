using LearnitySchool.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LearnitySchool.Web.ViewModels.Teacher.Submissions;

public class TeacherSubmissionListVm
{
    public string? Q { get; set; }
    public StudentSubmissionStatus? Status { get; set; } = StudentSubmissionStatus.PendingReview;
    public Guid? CourseId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public List<SelectListItem> Courses { get; set; } = new();
    public List<TeacherSubmissionRowVm> Items { get; set; } = new();
}

public class TeacherSubmissionRowVm
{
    public Guid SubmissionId { get; set; }
    public Guid CourseId { get; set; }
    public Guid LessonId { get; set; }
    public Guid TaskId { get; set; }

    public string StudentName { get; set; } = "—";
    public string? StudentEmail { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string LessonTitle { get; set; } = string.Empty;
    public string TaskTitle { get; set; } = string.Empty;
    public StudentSubmissionStatus Status { get; set; }
    public int? ServerSimilarity { get; set; }
    public DateTime SubmittedAtUtc { get; set; }
}
