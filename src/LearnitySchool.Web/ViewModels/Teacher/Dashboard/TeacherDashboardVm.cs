namespace LearnitySchool.Web.ViewModels.Teacher.Dashboard;

public class TeacherDashboardVm
{
    public int PendingSubmissions { get; set; }
    public int ApprovedSubmissions { get; set; }
    public int ReturnedSubmissions { get; set; }
    public int PaidLessonsCount { get; set; }
    public int UnpaidPaidRequiredCount { get; set; }
    public List<TeacherDashboardSubmissionRowVm> RecentSubmissions { get; set; } = new();
    public List<TeacherDashboardCourseRowVm> Courses { get; set; } = new();
}

public class TeacherDashboardSubmissionRowVm
{
    public Guid SubmissionId { get; set; }
    public string StudentName { get; set; } = "—";
    public string CourseTitle { get; set; } = "";
    public string LessonTitle { get; set; } = "";
    public string TaskTitle { get; set; } = "";
    public DateTime SubmittedAtUtc { get; set; }
}

public class TeacherDashboardCourseRowVm
{
    public Guid CourseId { get; set; }
    public string Title { get; set; } = "";
    public int StudentsCount { get; set; }
    public int LessonsCount { get; set; }
}
