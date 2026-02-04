using LearnitySchool.Domain.Enums;

namespace LearnitySchool.Web.ViewModels.Manager.Tasks;

public class TaskListItemVm
{
    public Guid Id { get; set; }
    public int Order { get; set; }
    public string Title { get; set; } = "";
    public LessonTaskType Type { get; set; }
    public bool IsPublished { get; set; }
}
