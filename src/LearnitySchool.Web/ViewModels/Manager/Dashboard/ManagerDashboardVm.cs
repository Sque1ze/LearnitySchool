using LearnitySchool.Domain.Enums;

namespace LearnitySchool.Web.ViewModels.Manager.Dashboard;

public class ManagerDashboardVm
{
    public int StudentsCount { get; set; }
    public int TeachersCount { get; set; }
    public int ManagersCount { get; set; }
    public int CoursesCount { get; set; }
    public int PublishedCoursesCount { get; set; }
    public int LessonsCount { get; set; }
    public int TasksCount { get; set; }
    public int PendingSubmissionsCount { get; set; }
    public int PendingPaymentsCount { get; set; }
    public int UnreadNotificationsCount { get; set; }

    public List<ManagerCourseDashboardRowVm> RecentCourses { get; set; } = new();
    public List<ManagerSubmissionDashboardRowVm> PendingSubmissions { get; set; } = new();
}

public class ManagerCourseDashboardRowVm
{
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public int LessonsCount { get; set; }
    public int StudentsCount { get; set; }
    public int TeachersCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ManagerSubmissionDashboardRowVm
{
    public Guid SubmissionId { get; set; }
    public string StudentName { get; set; } = "—";
    public string CourseTitle { get; set; } = string.Empty;
    public string LessonTitle { get; set; } = string.Empty;
    public string TaskTitle { get; set; } = string.Empty;
    public StudentSubmissionStatus Status { get; set; }
    public DateTime SubmittedAtUtc { get; set; }
}
