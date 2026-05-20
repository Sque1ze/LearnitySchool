using LearnitySchool.Infrastructure.Identity;
using LearnitySchool.Web.ViewModels.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LearnitySchool.Web.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public ProfileController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var roles = await _userManager.GetRolesAsync(user);
        return View(new ProfileDetailsVm
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            Age = user.Age,
            Role = roles.FirstOrDefault() ?? "—",
            CreatedAtUtc = user.CreatedAtUtc
        });
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        return View(new ProfileEditVm
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            UserName = user.UserName ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            Age = user.Age
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProfileEditVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        user.FirstName = vm.FirstName.Trim();
        user.LastName = vm.LastName.Trim();
        user.PhoneNumber = vm.PhoneNumber?.Trim();
        user.Age = vm.Age;

        if (!string.Equals(user.UserName, vm.UserName, StringComparison.OrdinalIgnoreCase))
        {
            var setName = await _userManager.SetUserNameAsync(user, vm.UserName.Trim());
            if (!setName.Succeeded)
            {
                foreach (var error in setName.Errors) ModelState.AddModelError(string.Empty, error.Description);
                return View(vm);
            }
        }

        var update = await _userManager.UpdateAsync(user);
        if (!update.Succeeded)
        {
            foreach (var error in update.Errors) ModelState.AddModelError(string.Empty, error.Description);
            return View(vm);
        }

        await _signInManager.RefreshSignInAsync(user);
        TempData["Success"] = "Профіль оновлено ✅";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult ChangePassword() => View(new ChangePasswordVm());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var result = await _userManager.ChangePasswordAsync(user, vm.OldPassword, vm.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
            return View(vm);
        }

        await _signInManager.RefreshSignInAsync(user);
        TempData["Success"] = "Пароль змінено ✅";
        return RedirectToAction(nameof(Index));
    }
}
