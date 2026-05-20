using System.ComponentModel.DataAnnotations;

namespace LearnitySchool.Web.ViewModels.Manager.Users;

public class UserEditVm
{
    public string Id { get; set; } = "";

    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, StringLength(256, MinimumLength = 2)]
    public string UserName { get; set; } = "";

    [Required, StringLength(50, MinimumLength = 2)]
    public string FirstName { get; set; } = "";

    [Required, StringLength(50, MinimumLength = 2)]
    public string LastName { get; set; } = "";

    [Range(4, 100, ErrorMessage = "Вік має бути від 4 до 100.")]
    public int? Age { get; set; }

    [Phone]
    [StringLength(40)]
    public string PhoneNumber { get; set; } = "";

    [Required]
    public string Role { get; set; } = "";
    public List<string> AvailableRoles { get; set; } = new();
}
