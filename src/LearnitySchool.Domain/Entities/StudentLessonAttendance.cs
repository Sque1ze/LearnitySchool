namespace LearnitySchool.Domain.Entities;

public enum AttendanceStatus
{
    Unknown = 0,
    Present = 1,
    Absent = 2
}

public class StudentLessonAttendance
{
    public Guid Id { get; set; }

    public Guid CourseId { get; set; }
    public Guid LessonId { get; set; }

    public string StudentUserId { get; set; } = string.Empty;

    public AttendanceStatus Status { get; set; } = AttendanceStatus.Unknown;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public string UpdatedByTeacherUserId { get; set; } = string.Empty;
}
