using System.ComponentModel.DataAnnotations;

namespace LearnitySchool.Web.ViewModels.Manager.Users;

public class UserCreateVm
{
    [Required(ErrorMessage = "Email обов’язковий.")]
    [EmailAddress(ErrorMessage = "Некоректний email.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Пароль обов’язковий.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль має містити мінімум 6 символів.")]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Ім’я обов’язкове.")]
    [StringLength(50, MinimumLength = 2)]
    public string FirstName { get; set; } = "";

    [Required(ErrorMessage = "Прізвище обов’язкове.")]
    [StringLength(50, MinimumLength = 2)]
    public string LastName { get; set; } = "";

    [Range(4, 100, ErrorMessage = "Вік має бути від 4 до 100.")]
    public int? Age { get; set; }

    [Phone(ErrorMessage = "Некоректний номер телефону.")]
    [StringLength(40)]
    public string PhoneNumber { get; set; } = "";

    [Required(ErrorMessage = "Оберіть роль.")]
    public string Role { get; set; } = "";

    public List<string> AvailableRoles { get; set; } = new();
}
