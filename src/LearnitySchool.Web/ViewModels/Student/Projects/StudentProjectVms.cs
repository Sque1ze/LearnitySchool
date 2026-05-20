using System.ComponentModel.DataAnnotations;

namespace LearnitySchool.Web.ViewModels.Student.Projects;

public class StudentProjectListVm
{
    public List<StudentProjectRowVm> Items { get; set; } = new();
}

public class StudentProjectRowVm
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public class StudentProjectEditVm
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Назва проєкту обов’язкова.")]
    [StringLength(160, MinimumLength = 2)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    public string Html { get; set; } = "<!-- HTML -->";
    public string Css { get; set; } = "/* CSS */";
    public string Js { get; set; } = "// JS";

    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public class StudentProjectSaveVm
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Html { get; set; }
    public string? Css { get; set; }
    public string? Js { get; set; }
}
