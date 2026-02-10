namespace LearnitySchool.Web.ViewModels.Teacher;

public class TeacherStaffVm
{
    public string? Q { get; set; }          
    public string? Login { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }       

    public List<StaffRowVm> Items { get; set; } = new();

    public class StaffRowVm
    {
        public string UserId { get; set; } = "";
        public string FullName { get; set; } = "";
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Login { get; set; }
        public string Role { get; set; } = "";
    }
}
