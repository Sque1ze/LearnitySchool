using System;
using System.ComponentModel.DataAnnotations;

namespace LearnitySchool.Web.ViewModels.Manager.Practice;

public class PracticeEditVm
{
    public Guid TaskId { get; set; }
    public Guid LessonId { get; set; }
    public Guid CourseId { get; set; }

    public string TaskTitle { get; set; } = "";

    [Required]
    public string Statement { get; set; } = "";

    [Range(0, 100)]
    public int SimilarityThreshold { get; set; } = 80;

    // Starter
    public string StarterHtml { get; set; } = "";
    public string StarterCss { get; set; } = "";
    public string StarterJs { get; set; } = "";

    // Reference
    public string ReferenceHtml { get; set; } = "";
    public string ReferenceCss { get; set; } = "";
    public string ReferenceJs { get; set; } = "";
}
