using LearnitySchool.Application.Common;
using LearnitySchool.Infrastructure.Identity;
using LearnitySchool.Web.ViewModels.Manager.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearnitySchool.Web.Controllers;

[Authorize(Roles = RoleNames.Manager)]
public class ManagerUsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public ManagerUsersController(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    private async Task<List<string>> GetAllRolesAsync()
    {
        return await _roleManager.Roles
            .Where(r => r.Name != null)
            .Select(r => r.Name!)
            .OrderBy(x => x)
            .ToListAsync();
    }

    // GET: /ManagerUsers
    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users
            .OrderBy(u => u.Email)
            .Select(u => new UserListItemVm
            {
                Id = u.Id,
                Email = u.Email ?? "",
                UserName = u.UserName ?? "",
                FirstName = u.FirstName,
                LastName = u.LastName,
                Age = u.Age,
                PhoneNumber = u.PhoneNumber ?? "",
                Role = "" // заповнимо нижче
            })
            .ToListAsync();

        // 1 роль на юзера (показуємо першу)
        foreach (var u in users)
        {
            var user = await _userManager.FindByIdAsync(u.Id);
            if (user == null) continue;

            var roles = await _userManager.GetRolesAsync(user);
            u.Role = roles.FirstOrDefault() ?? "";
        }

        return View(users);
    }

    // GET: /ManagerUsers/Details/{id}
    public async Task<IActionResult> Details(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);

        var vm = new UserEditVm
        {
            Id = user.Id,
            Email = user.Email ?? "",
            UserName = user.UserName ?? "",
            FirstName = user.FirstName,
            LastName = user.LastName,
            Age = user.Age,
            PhoneNumber = user.PhoneNumber ?? "",

            Role = roles.FirstOrDefault() ?? "",
            AvailableRoles = await GetAllRolesAsync()
        };

        return View(vm);
    }

    // GET: /ManagerUsers/Create
    public async Task<IActionResult> Create()
    {
        var vm = new UserCreateVm
        {
            AvailableRoles = await GetAllRolesAsync(),
            Role = RoleNames.Student // дефолт
        };

        return View(vm);
    }

    // POST: /ManagerUsers/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserCreateVm vm)
    {
        vm.AvailableRoles = await GetAllRolesAsync();

        if (!ModelState.IsValid) return View(vm);

        var user = new ApplicationUser
        {
            UserName = vm.Email,
            Email = vm.Email,
            FirstName = vm.FirstName,
            LastName = vm.LastName,
            Age = vm.Age,
            PhoneNumber = vm.PhoneNumber
        };

        var result = await _userManager.CreateAsync(user, vm.Password);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError("", e.Description);

            return View(vm);
        }

        // роль
        var role = vm.Role?.Trim();
        if (!string.IsNullOrWhiteSpace(role))
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                ModelState.AddModelError("", $"Роль '{role}' не існує.");
                return View(vm);
            }

            var addRole = await _userManager.AddToRoleAsync(user, role);
            if (!addRole.Succeeded)
            {
                foreach (var e in addRole.Errors)
                    ModelState.AddModelError("", e.Description);

                return View(vm);
            }
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: /ManagerUsers/Edit/{id}
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);

        var vm = new UserEditVm
        {
            Id = user.Id,
            Email = user.Email ?? "",
            UserName = user.UserName ?? "",
            FirstName = user.FirstName,
            LastName = user.LastName,
            Age = user.Age,
            PhoneNumber = user.PhoneNumber ?? "",

            Role = roles.FirstOrDefault() ?? "",
            AvailableRoles = await GetAllRolesAsync()
        };

        return View(vm);
    }

    // POST: /ManagerUsers/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserEditVm vm)
    {
        vm.AvailableRoles = await GetAllRolesAsync();

        if (!ModelState.IsValid) return View(vm);

        var user = await _userManager.FindByIdAsync(vm.Id);
        if (user == null) return NotFound();

        // 1) поля
        user.FirstName = vm.FirstName;
        user.LastName = vm.LastName;
        user.Age = vm.Age;
        user.PhoneNumber = vm.PhoneNumber;

        // 2) email / username
        if (!string.Equals(user.Email, vm.Email, StringComparison.OrdinalIgnoreCase))
        {
            var setEmail = await _userManager.SetEmailAsync(user, vm.Email);
            if (!setEmail.Succeeded)
            {
                foreach (var e in setEmail.Errors) ModelState.AddModelError("", e.Description);
                return View(vm);
            }
        }

        if (!string.Equals(user.UserName, vm.UserName, StringComparison.OrdinalIgnoreCase))
        {
            var setUserName = await _userManager.SetUserNameAsync(user, vm.UserName);
            if (!setUserName.Succeeded)
            {
                foreach (var e in setUserName.Errors) ModelState.AddModelError("", e.Description);
                return View(vm);
            }
        }

        // 3) роль (вважаємо 1 роль на юзера)
        var currentRoles = await _userManager.GetRolesAsync(user);
        var currentRole = currentRoles.FirstOrDefault();
        var newRole = vm.Role?.Trim();

        if (!string.IsNullOrWhiteSpace(newRole) && newRole != currentRole)
        {
            if (!await _roleManager.RoleExistsAsync(newRole))
            {
                ModelState.AddModelError("", $"Роль '{newRole}' не існує.");
                return View(vm);
            }

            // страховка: не можна забрати роль Manager у самого себе
            if (user.Id == _userManager.GetUserId(User) &&
                currentRole == RoleNames.Manager &&
                newRole != RoleNames.Manager)
            {
                ModelState.AddModelError("", "Не можна забрати роль Manager у самого себе.");
                return View(vm);
            }

            if (!string.IsNullOrWhiteSpace(currentRole))
            {
                var remove = await _userManager.RemoveFromRoleAsync(user, currentRole);
                if (!remove.Succeeded)
                {
                    foreach (var e in remove.Errors) ModelState.AddModelError("", e.Description);
                    return View(vm);
                }
            }

            var add = await _userManager.AddToRoleAsync(user, newRole);
            if (!add.Succeeded)
            {
                foreach (var e in add.Errors) ModelState.AddModelError("", e.Description);
                return View(vm);
            }
        }

        // 4) update
        var update = await _userManager.UpdateAsync(user);
        if (!update.Succeeded)
        {
            foreach (var e in update.Errors) ModelState.AddModelError("", e.Description);
            return View(vm);
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: /ManagerUsers/Delete/{id}
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);

        var vm = new UserEditVm
        {
            Id = user.Id,
            Email = user.Email ?? "",
            UserName = user.UserName ?? "",
            FirstName = user.FirstName,
            LastName = user.LastName,
            Age = user.Age,
            PhoneNumber = user.PhoneNumber ?? "",

            Role = roles.FirstOrDefault() ?? "",
            AvailableRoles = await GetAllRolesAsync()
        };

        return View(vm);
    }

    // POST: /ManagerUsers/DeleteConfirmed
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        // не можна видалити самого себе
        if (user.Id == _userManager.GetUserId(User))
        {
            TempData["Error"] = "Не можна видалити самого себе.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            TempData["Error"] = string.Join("; ", result.Errors.Select(e => e.Description));
        }

        return RedirectToAction(nameof(Index));
    }
}
