namespace LearnitySchool.Web.ViewModels.Manager.Users;

public class UsersIndexVm
{
    public string? Q { get; set; }          
    public string? Role { get; set; }       

    public List<string> AvailableRoles { get; set; } = new();
    public List<UserListItemVm> Users { get; set; } = new();
}
