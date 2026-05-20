using System.ComponentModel.DataAnnotations;

namespace LearnitySchool.Web.ViewModels.Profile;

public class ProfileDetailsVm
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public int? Age { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}

public class ProfileEditVm
{
    [Required, StringLength(50, MinimumLength = 2)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(50, MinimumLength = 2)]
    public string LastName { get; set; } = string.Empty;

    [Required, StringLength(256, MinimumLength = 2)]
    public string UserName { get; set; } = string.Empty;

    [Phone, StringLength(40)]
    public string? PhoneNumber { get; set; }

    [Range(4, 100)]
    public int? Age { get; set; }
}

public class ChangePasswordVm
{
    [Required(ErrorMessage = "Введіть старий пароль.")]
    [DataType(DataType.Password)]
    public string OldPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть новий пароль.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль має містити мінімум 6 символів.")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Підтвердіть новий пароль.")]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Паролі не збігаються.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
