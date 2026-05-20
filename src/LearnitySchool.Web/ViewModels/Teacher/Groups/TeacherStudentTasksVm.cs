using LearnitySchool.Domain.Enums;

namespace LearnitySchool.Web.ViewModels.Teacher.Groups;

public class TeacherStudentTasksVm
{
    public Guid CourseId { get; set; }
    public string StudentUserId { get; set; } = string.Empty;
    public string StudentName { get; set; } = "—";

    public List<LessonBlockVm> Lessons { get; set; } = new();

    public class LessonBlockVm
    {
        public Guid LessonId { get; set; }
        public int Order { get; set; }
        public string Title { get; set; } = string.Empty;

        public List<TaskSquareVm> Tasks { get; set; } = new();
    }

    public class TaskSquareVm
    {
        public Guid TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsCompleted { get; set; }
        public TaskAssessmentMode AssessmentMode { get; set; }
        public StudentSubmissionStatus? SubmissionStatus { get; set; }
        public bool HasQuizAttempt { get; set; }
        public int? QuizScorePercent { get; set; }
    }
}
