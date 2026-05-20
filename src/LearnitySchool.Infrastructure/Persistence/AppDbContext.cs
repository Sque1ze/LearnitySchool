using LearnitySchool.Domain.Entities;
using LearnitySchool.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace LearnitySchool.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Group> Groups => Set<Group>();

    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<CourseSchedule> CourseSchedules => Set<CourseSchedule>();

    public DbSet<CourseTeacher> CourseTeachers => Set<CourseTeacher>();
    public DbSet<CourseStudent> CourseStudents => Set<CourseStudent>();

    public DbSet<LessonTask> LessonTasks => Set<LessonTask>();
    public DbSet<StudentTaskProgress> StudentTaskProgresses => Set<StudentTaskProgress>();

    public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();
    public DbSet<QuizOption> QuizOptions => Set<QuizOption>();
    public DbSet<StudentQuizAttempt> StudentQuizAttempts => Set<StudentQuizAttempt>();
    public DbSet<PracticeTask> PracticeTasks => Set<PracticeTask>();
    public DbSet<StudentPracticeDraft> StudentPracticeDrafts => Set<StudentPracticeDraft>();
    public DbSet<StudentTaskSubmission> StudentTaskSubmissions => Set<StudentTaskSubmission>();
    public DbSet<LessonPayment> LessonPayments => Set<LessonPayment>();
    public DbSet<StudentProject> StudentProjects => Set<StudentProject>();
    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<CourseLessonGate> CourseLessonGates => Set<CourseLessonGate>();
    public DbSet<StudentLessonAttendance> StudentLessonAttendances => Set<StudentLessonAttendance>();
    public DbSet<LessonAccess> LessonAccesses => Set<LessonAccess>();



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

            b.Property(x => x.ManagerUserId)
                .HasMaxLength(450);
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

            b.Property(x => x.PriceAmount)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            b.Property(x => x.Currency)
                .HasMaxLength(8)
                .IsRequired();

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

            b.Property(x => x.AssessmentMode)
                .IsRequired();

            b.Property(x => x.QuizType)
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
            b.Property(x => x.QuizType).IsRequired();
            b.Property(x => x.AnswersJson);
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

        builder.Entity<StudentPracticeDraft>(b =>
        {
            b.HasKey(x => x.Id);

            b.Property(x => x.StudentUserId)
                .HasMaxLength(450)
                .IsRequired();

            b.Property(x => x.UpdatedAt)
                .IsRequired();

            // HTML/CSS/JS можуть бути великими => nvarchar(max) (за замовчуванням і так ок)
            b.Property(x => x.Html);
            b.Property(x => x.Css);
            b.Property(x => x.Js);

            // ✅ 1 draft на 1 студента на 1 задачу
            b.HasIndex(x => new { x.LessonTaskId, x.StudentUserId })
                .IsUnique();

            // FK на LessonTask
            b.HasOne(x => x.LessonTask)
                .WithMany() // можна .WithMany() бо в LessonTask у тебе немає Drafts navigation
                .HasForeignKey(x => x.LessonTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            // Корисні індекси для вибірок
            b.HasIndex(x => x.StudentUserId);
            b.HasIndex(x => x.LessonTaskId);
        });


        builder.Entity<StudentTaskSubmission>(b =>
        {
            b.HasKey(x => x.Id);

            b.Property(x => x.StudentUserId)
                .HasMaxLength(450)
                .IsRequired();

            b.Property(x => x.Html).IsRequired();
            b.Property(x => x.Css).IsRequired();
            b.Property(x => x.Js).IsRequired();

            b.Property(x => x.Status).IsRequired();
            b.Property(x => x.SubmittedAtUtc).IsRequired();

            b.Property(x => x.ReviewedByTeacherUserId)
                .HasMaxLength(450);

            b.Property(x => x.TeacherComment)
                .HasMaxLength(2000);

            // 1 актуальна здача на 1 студента на 1 задачу.
            // Якщо вчитель повернув роботу, студент перездає її в цей самий запис.
            b.HasIndex(x => new { x.LessonTaskId, x.StudentUserId })
                .IsUnique();

            b.HasIndex(x => x.StudentUserId);
            b.HasIndex(x => x.LessonTaskId);
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.SubmittedAtUtc);

            b.HasOne(x => x.LessonTask)
                .WithMany()
                .HasForeignKey(x => x.LessonTaskId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<LessonPayment>(b =>
        {
            b.HasKey(x => x.Id);

            b.Property(x => x.StudentId)
                .HasMaxLength(450)
                .IsRequired();

            b.Property(x => x.Amount)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            b.Property(x => x.Currency)
                .HasMaxLength(8)
                .IsRequired();

            b.Property(x => x.Status)
                .IsRequired();

            b.Property(x => x.PaymentProvider)
                .HasMaxLength(64)
                .IsRequired();

            b.Property(x => x.ProviderPaymentId)
                .HasMaxLength(128);

            b.HasIndex(x => new { x.StudentId, x.LessonId });
            b.HasIndex(x => new { x.StudentId, x.LessonId, x.Status });
            b.HasIndex(x => x.CreatedAtUtc);

            b.HasOne(x => x.Lesson)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.LessonId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<StudentProject>(b =>
        {
            b.HasKey(x => x.Id);

            b.Property(x => x.StudentId)
                .HasMaxLength(450)
                .IsRequired();

            b.Property(x => x.Title)
                .HasMaxLength(160)
                .IsRequired();

            b.Property(x => x.Description)
                .HasMaxLength(1000);

            b.Property(x => x.CreatedAtUtc).IsRequired();
            b.Property(x => x.UpdatedAtUtc).IsRequired();

            b.HasIndex(x => x.StudentId);
            b.HasIndex(x => new { x.StudentId, x.UpdatedAtUtc });
        });

        builder.Entity<Notification>(b =>
        {
            b.HasKey(x => x.Id);

            b.Property(x => x.UserId)
                .HasMaxLength(450)
                .IsRequired();

            b.Property(x => x.Title)
                .HasMaxLength(160)
                .IsRequired();

            b.Property(x => x.Message)
                .HasMaxLength(1000)
                .IsRequired();

            b.Property(x => x.Type).IsRequired();
            b.Property(x => x.IsRead).IsRequired();
            b.Property(x => x.CreatedAtUtc).IsRequired();

            b.Property(x => x.Url)
                .HasMaxLength(500);

            b.HasIndex(x => new { x.UserId, x.IsRead });
            b.HasIndex(x => x.CreatedAtUtc);
        });

        builder.Entity<CourseLessonGate>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => new { x.CourseId, x.LessonId }).IsUnique();
            b.Property(x => x.UpdatedByTeacherUserId).HasMaxLength(450).IsRequired();
        });

        builder.Entity<StudentLessonAttendance>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => new { x.CourseId, x.LessonId, x.StudentUserId }).IsUnique();
            b.Property(x => x.StudentUserId).HasMaxLength(450).IsRequired();
            b.Property(x => x.UpdatedByTeacherUserId).HasMaxLength(450).IsRequired();
        });

        builder.Entity<LessonAccess>(b =>
        {
            b.HasKey(x => new { x.CourseId, x.LessonId });

            b.Property(x => x.IsOpen).IsRequired();

            b.HasOne(x => x.Course)
                .WithMany()
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ тут НЕ Cascade, інакше multiple cascade paths
            b.HasOne(x => x.Lesson)
                .WithMany()
                .HasForeignKey(x => x.LessonId)
                .OnDelete(DeleteBehavior.NoAction);

            b.HasIndex(x => x.LessonId);
        });
    }
}
