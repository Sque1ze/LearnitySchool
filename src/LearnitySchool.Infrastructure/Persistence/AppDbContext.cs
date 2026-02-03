using LearnitySchool.Domain.Entities;
using LearnitySchool.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LearnitySchool.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Group> Groups => Set<Group>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Group>(b =>
        {
            b.Property(x => x.Name).HasMaxLength(120).IsRequired();
            b.Property(x => x.TeacherUserId).HasMaxLength(450).IsRequired();
        });
    }
}
