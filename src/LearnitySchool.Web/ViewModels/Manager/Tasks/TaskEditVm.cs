using LearnitySchool.Domain.Enums;

namespace LearnitySchool.Web.ViewModels.Manager.Tasks;

public class TaskEditVm
{
    public Guid Id { get; set; }

    public Guid LessonId { get; set; }

    public int Order { get; set; } = 1;
    public string Title { get; set; } = "";
    public string? Description { get; set; }

    public LessonTaskType Type { get; set; } = LessonTaskType.Quiz;

    public bool IsPublished { get; set; }

    // для заголовків на сторінках
    public string LessonTitle { get; set; } = "";
}
