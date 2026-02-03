using LearnitySchool.Application;
using LearnitySchool.Infrastructure;
using LearnitySchool.Infrastructure.Identity;
using LearnitySchool.Web.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LearnitySchool.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Identity UI pages (login/register) - optional
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Seed roles + demo users (optional)
await SeedIdentityAsync(app);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

static async Task SeedIdentityAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Ensure DB exists (for first run). In real projects: use migrations.
    await db.Database.EnsureCreatedAsync();

    string[] roles = [RoleNames.Student, RoleNames.Teacher, RoleNames.Manager];

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Demo accounts for quick testing
    await EnsureUserAsync(userManager, "student@learnity.local", "Student123!", RoleNames.Student);
    await EnsureUserAsync(userManager, "teacher@learnity.local", "Teacher123!", RoleNames.Teacher);
    await EnsureUserAsync(userManager, "manager@learnity.local", "Manager123!", RoleNames.Manager);
}

static async Task EnsureUserAsync(UserManager<ApplicationUser> userManager, string email, string password, string role)
{
    var user = await userManager.FindByEmailAsync(email);
    if (user is null)
    {
        user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
        var create = await userManager.CreateAsync(user, password);
        if (!create.Succeeded) return;
    }

    if (!await userManager.IsInRoleAsync(user, role))
        await userManager.AddToRoleAsync(user, role);
}
