using System;
using LearnitySchool.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearnitySchool.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260508123000_AddPaymentsProjectsNotificationsAndQuizTypes")]
public partial class AddPaymentsProjectsNotificationsAndQuizTypes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "CreatedAtUtc",
            table: "AspNetUsers",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<decimal>(
            name: "PriceAmount",
            table: "Lessons",
            type: "decimal(10,2)",
            nullable: false,
            defaultValue: 0m);

        migrationBuilder.AddColumn<string>(
            name: "Currency",
            table: "Lessons",
            type: "nvarchar(8)",
            maxLength: 8,
            nullable: false,
            defaultValue: "UAH");

        migrationBuilder.AddColumn<int>(
            name: "QuizType",
            table: "LessonTasks",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "QuizType",
            table: "StudentQuizAttempts",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<string>(
            name: "AnswersJson",
            table: "StudentQuizAttempts",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "LessonPayments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                StudentId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                LessonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                Currency = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                Status = table.Column<int>(type: "int", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                PaidAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                PaymentProvider = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                ProviderPaymentId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LessonPayments", x => x.Id);
                table.ForeignKey(
                    name: "FK_LessonPayments_Lessons_LessonId",
                    column: x => x.LessonId,
                    principalTable: "Lessons",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "StudentProjects",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                StudentId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                Title = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                Html = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Css = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Js = table.Column<string>(type: "nvarchar(max)", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StudentProjects", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Notifications",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                Title = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                Type = table.Column<int>(type: "int", nullable: false),
                IsRead = table.Column<bool>(type: "bit", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                ReadAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Notifications", x => x.Id);
            });

        migrationBuilder.CreateIndex(name: "IX_LessonPayments_CreatedAtUtc", table: "LessonPayments", column: "CreatedAtUtc");
        migrationBuilder.CreateIndex(name: "IX_LessonPayments_LessonId", table: "LessonPayments", column: "LessonId");
        migrationBuilder.CreateIndex(name: "IX_LessonPayments_StudentId_LessonId", table: "LessonPayments", columns: new[] { "StudentId", "LessonId" });
        migrationBuilder.CreateIndex(name: "IX_LessonPayments_StudentId_LessonId_Status", table: "LessonPayments", columns: new[] { "StudentId", "LessonId", "Status" });
        migrationBuilder.CreateIndex(name: "IX_StudentProjects_StudentId", table: "StudentProjects", column: "StudentId");
        migrationBuilder.CreateIndex(name: "IX_StudentProjects_StudentId_UpdatedAtUtc", table: "StudentProjects", columns: new[] { "StudentId", "UpdatedAtUtc" });
        migrationBuilder.CreateIndex(name: "IX_Notifications_CreatedAtUtc", table: "Notifications", column: "CreatedAtUtc");
        migrationBuilder.CreateIndex(name: "IX_Notifications_UserId_IsRead", table: "Notifications", columns: new[] { "UserId", "IsRead" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Notifications");
        migrationBuilder.DropTable(name: "StudentProjects");
        migrationBuilder.DropTable(name: "LessonPayments");

        migrationBuilder.DropColumn(name: "CreatedAtUtc", table: "AspNetUsers");
        migrationBuilder.DropColumn(name: "PriceAmount", table: "Lessons");
        migrationBuilder.DropColumn(name: "Currency", table: "Lessons");
        migrationBuilder.DropColumn(name: "QuizType", table: "LessonTasks");
        migrationBuilder.DropColumn(name: "QuizType", table: "StudentQuizAttempts");
        migrationBuilder.DropColumn(name: "AnswersJson", table: "StudentQuizAttempts");
    }
}
