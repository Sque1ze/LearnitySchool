namespace LearnitySchool.Web.ViewModels.Teacher.Students;

public class TeacherStudentsVm
{
    public string? Q { get; set; }           
    public int? AgeFrom { get; set; }
    public int? AgeTo { get; set; }
    public string? Phone { get; set; }
    public string? Login { get; set; }

    public List<TeacherStudentRowVm> Items { get; set; } = new();

    public class TeacherStudentRowVm
    {
        public string StudentUserId { get; set; } = "";
        public string FullName { get; set; } = "—";
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int? Age { get; set; }
        public string? Login { get; set; }

        public int GroupsCount { get; set; }
    }
}
