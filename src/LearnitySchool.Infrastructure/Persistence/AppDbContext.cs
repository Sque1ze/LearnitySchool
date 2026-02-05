using LearnitySchool.Domain.Entities;
using LearnitySchool.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LearnitySchool.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    // ===== Existing =====
    public DbSet<Group> Groups => Set<Group>();

    // ===== Courses =====
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<CourseSchedule> CourseSchedules => Set<CourseSchedule>();

    // ===== Course ↔ People =====
    public DbSet<CourseTeacher> CourseTeachers => Set<CourseTeacher>();
    public DbSet<CourseStudent> CourseStudents => Set<CourseStudent>();

    // ===== Lessons ↔ Tasks =====
    public DbSet<LessonTask> LessonTasks => Set<LessonTask>();
    public DbSet<StudentTaskProgress> StudentTaskProgresses => Set<StudentTaskProgress>();

    public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();
    public DbSet<QuizOption> QuizOptions => Set<QuizOption>();
    public DbSet<StudentQuizAttempt> StudentQuizAttempts => Set<StudentQuizAttempt>();
    public DbSet<PracticeTask> PracticeTasks => Set<PracticeTask>();


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // =========================
        // Group
        // =========================
        builder.Entity<Group>(b =>
        {
            b.Property(x => x.Name)
                .HasMaxLength(120)
                .IsRequired();

            b.Property(x => x.TeacherUserId)
                .HasMaxLength(450)
                .IsRequired();
        });

        // =========================
        // Course
        // =========================
        builder.Entity<Course>(b =>
        {
            b.Property(x => x.Title)
                .HasMaxLength(200)
                .IsRequired();

            b.Property(x => x.Description)
                .HasMaxLength(4000);

            b.Property(x => x.CreatedAt)
                .IsRequired();

            b.HasMany(x => x.Lessons)
                .WithOne(x => x.Course)
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasMany(x => x.Schedules)
                .WithOne(x => x.Course)
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasMany(x => x.Teachers)
                .WithOne(x => x.Course)
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasMany(x => x.Students)
                .WithOne(x => x.Course)
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.Title);
        });

        // =========================
        // Lesson
        // =========================
        builder.Entity<Lesson>(b =>
        {
            b.Property(x => x.Title)
                .HasMaxLength(200)
                .IsRequired();

            b.Property(x => x.Description)
                .HasMaxLength(4000);

            // Content може бути великим => nvarchar(max)
            b.Property(x => x.Content);

            b.Property(x => x.Order)
                .IsRequired();

            // унікальний порядок уроків у межах курсу
            b.HasIndex(x => new { x.CourseId, x.Order })
                .IsUnique();
        });

        // =========================
        // CourseSchedule
        // =========================
        builder.Entity<CourseSchedule>(b =>
        {
            b.Property(x => x.DayOfWeek)
                .IsRequired();

            b.Property(x => x.StartTime)
                .IsRequired();

            b.Property(x => x.Duration)
                .IsRequired();

            // щоб не було дублікатів типу "Пн 19:00" в межах курсу
            b.HasIndex(x => new { x.CourseId, x.DayOfWeek, x.StartTime })
                .IsUnique();
        });

        // =========================
        // CourseTeacher
        // =========================
        builder.Entity<CourseTeacher>(b =>
        {
            b.HasKey(x => new { x.CourseId, x.TeacherUserId });

            b.Property(x => x.TeacherUserId)
                .HasMaxLength(450)
                .IsRequired();

            b.HasIndex(x => x.TeacherUserId);
        });

        // =========================
        // CourseStudent
        // =========================
        builder.Entity<CourseStudent>(b =>
        {
            b.HasKey(x => new { x.CourseId, x.StudentUserId });

            b.Property(x => x.StudentUserId)
                .HasMaxLength(450)
                .IsRequired();

            b.Property(x => x.EnrolledAt)
                .IsRequired();

            b.HasIndex(x => x.StudentUserId);
        });

        // =========================
        // LessonTask
        // =========================
        builder.Entity<LessonTask>(b =>
        {
            b.Property(x => x.Title)
                .HasMaxLength(200)
                .IsRequired();

            b.Property(x => x.Description)
                .HasMaxLength(4000);

            b.Property(x => x.Order)
                .IsRequired();

            b.Property(x => x.Type)
                .IsRequired();

            b.HasOne(x => x.Lesson)
                .WithMany(x => x.Tasks) // ✅ потрібна навігація в Lesson
                .HasForeignKey(x => x.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            // унікальний порядок задач в межах уроку
            b.HasIndex(x => new { x.LessonId, x.Order })
                .IsUnique();

            // швидкі вибірки задач по уроку
            b.HasIndex(x => x.LessonId);
        });

        // =========================
        // StudentTaskProgress
        // =========================
        builder.Entity<StudentTaskProgress>(b =>
        {
            b.Property(x => x.StudentUserId)
                .HasMaxLength(450)
                .IsRequired();

            b.Property(x => x.IsCompleted)
                .IsRequired();

            // 1 студент = 1 запис прогресу на 1 задачу
            b.HasIndex(x => new { x.StudentUserId, x.TaskId })
                .IsUnique();

            // швидко діставати прогрес по задачі
            b.HasIndex(x => x.TaskId);

            // звʼязок до LessonTask
            b.HasOne(x => x.Task)
                .WithMany()
                .HasForeignKey(x => x.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // =========================
        // QuizQuestion
        // =========================
        builder.Entity<QuizQuestion>(b =>
        {
            b.Property(x => x.Text).HasMaxLength(4000).IsRequired();
            b.Property(x => x.Order).IsRequired();

            b.HasMany(x => x.Options)
                .WithOne(x => x.Question)
                .HasForeignKey(x => x.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.TaskId, x.Order }).IsUnique();
        });

        // =========================
        // QuizOption
        // =========================
        builder.Entity<QuizOption>(b =>
        {
            b.Property(x => x.Text).HasMaxLength(2000).IsRequired();
            b.Property(x => x.Order).IsRequired();

            b.HasIndex(x => new { x.QuestionId, x.Order }).IsUnique();
        });

        // =========================
        // StudentQuizAttempt
        // =========================
        builder.Entity<StudentQuizAttempt>(b =>
        {
            b.Property(x => x.StudentUserId).HasMaxLength(450).IsRequired();
            b.Property(x => x.SubmittedAt).IsRequired();

            // 1 attempt per student per task (останній перезаписуємо)
            b.HasIndex(x => new { x.TaskId, x.StudentUserId }).IsUnique();
        });

        builder.Entity<PracticeTask>(b =>
        {
            b.HasKey(x => x.Id);

            b.Property(x => x.Statement)
                .IsRequired();

            b.Property(x => x.SimilarityThreshold)
                .IsRequired();

            b.HasOne(x => x.LessonTask)
                .WithOne()
                .HasForeignKey<PracticeTask>(x => x.LessonTaskId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
