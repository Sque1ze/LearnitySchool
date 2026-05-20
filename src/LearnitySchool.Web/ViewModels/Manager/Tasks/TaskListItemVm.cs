using LearnitySchool.Domain.Enums;

namespace LearnitySchool.Web.ViewModels.Manager.Tasks;

public class TaskListItemVm
{
    public Guid Id { get; set; }
    public int Order { get; set; }
    public string Title { get; set; } = string.Empty;
    public LessonTaskType Type { get; set; }
    public TaskAssessmentMode AssessmentMode { get; set; }
    public QuizType QuizType { get; set; }
    public bool IsPublished { get; set; }
}
