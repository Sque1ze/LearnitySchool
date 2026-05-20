using Microsoft.AspNetCore.Mvc.Rendering;

namespace LearnitySchool.Web.ViewModels.Teacher.Payments;

public class TeacherPaymentsVm
{
    public Guid? CourseId { get; set; }
    public List<SelectListItem> Courses { get; set; } = new();
    public List<TeacherPaymentRowVm> Items { get; set; } = new();
}

public class TeacherPaymentRowVm
{
    public string StudentName { get; set; } = "—";
    public string? StudentEmail { get; set; }
    public string CourseTitle { get; set; } = "";
    public int LessonOrder { get; set; }
    public string LessonTitle { get; set; } = "";
    public decimal PriceAmount { get; set; }
    public string Currency { get; set; } = "UAH";
    public bool IsPaid { get; set; }
    public DateTime? PaidAtUtc { get; set; }
}
