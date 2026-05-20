using System;
using LearnitySchool.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearnitySchool.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260508093000_AddTaskSubmissionReviewFlow")]
public partial class AddTaskSubmissionReviewFlow : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "AssessmentMode",
            table: "LessonTasks",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.CreateTable(
            name: "StudentTaskSubmissions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                LessonTaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                StudentUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                Html = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Css = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Js = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ClientSimilarity = table.Column<int>(type: "int", nullable: true),
                ServerSimilarity = table.Column<int>(type: "int", nullable: true),
                Status = table.Column<int>(type: "int", nullable: false),
                SubmittedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                ReviewedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                ReviewedByTeacherUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                TeacherComment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StudentTaskSubmissions", x => x.Id);
                table.ForeignKey(
                    name: "FK_StudentTaskSubmissions_LessonTasks_LessonTaskId",
                    column: x => x.LessonTaskId,
                    principalTable: "LessonTasks",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_StudentTaskSubmissions_LessonTaskId",
            table: "StudentTaskSubmissions",
            column: "LessonTaskId");

        migrationBuilder.CreateIndex(
            name: "IX_StudentTaskSubmissions_LessonTaskId_StudentUserId",
            table: "StudentTaskSubmissions",
            columns: new[] { "LessonTaskId", "StudentUserId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_StudentTaskSubmissions_Status",
            table: "StudentTaskSubmissions",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_StudentTaskSubmissions_StudentUserId",
            table: "StudentTaskSubmissions",
            column: "StudentUserId");

        migrationBuilder.CreateIndex(
            name: "IX_StudentTaskSubmissions_SubmittedAtUtc",
            table: "StudentTaskSubmissions",
            column: "SubmittedAtUtc");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "StudentTaskSubmissions");

        migrationBuilder.DropColumn(
            name: "AssessmentMode",
            table: "LessonTasks");
    }
}
