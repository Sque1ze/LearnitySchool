namespace LearnitySchool.Web.ViewModels.Manager.Users;

public class UserEditVm
{
    public string Id { get; set; } = "";

    public string Email { get; set; } = "";
    public string UserName { get; set; } = "";

    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";

    public int? Age { get; set; }
    public string PhoneNumber { get; set; } = "";

    public string Role { get; set; } = "";
    public List<string> AvailableRoles { get; set; } = new();
}
