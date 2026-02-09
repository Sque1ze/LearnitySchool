namespace LearnitySchool.Web.ViewModels.Teacher.Groups;

public class TeacherGroupsVm
{
    // filters (GET)
    public string? Q { get; set; }                // пошук по назві групи/курсу
    public string? TeacherId { get; set; }
    public string? ManagerId { get; set; }
    public int? StudentsCount { get; set; }

    // dropdown data
    public List<FilterOptionVm> Teachers { get; set; } = new();
    public List<FilterOptionVm> Managers { get; set; } = new();

    // results
    public List<TeacherGroupRowVm> Items { get; set; } = new();
}

public class TeacherGroupRowVm
{
    public Guid CourseId { get; set; }
    public string Title { get; set; } = "";
    public int StudentsCount { get; set; }
    public string TeacherName { get; set; } = "";
    public string? ManagerName { get; set; }
    public bool IsPublished { get; set; }         // статус
    public string Format { get; set; } = "Онлайн"; // поки статично
}

public class FilterOptionVm
{
    public string Id { get; set; } = "";
    public string Text { get; set; } = "";
}
