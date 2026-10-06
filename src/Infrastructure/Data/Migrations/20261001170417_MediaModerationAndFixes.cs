using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitectureBase.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class MediaModerationAndFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TestGrades_UserId",
                table: "TestGrades");

            migrationBuilder.DropIndex(
                name: "IX_AttemptQuestions_AttemptId",
                table: "AttemptQuestions");

            migrationBuilder.AlterColumn<float>(
                name: "Score",
                table: "TestGrades",
                type: "numeric(9,2)",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "numeric(5,2)");

            migrationBuilder.AddColumn<Guid>(
                name: "MediaId",
                table: "Questions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MediaType",
                table: "Questions",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CanUploadImage",
                table: "Plans",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanUploadVideo",
                table: "Plans",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "BanReason",
                table: "DomainUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "BannedAt",
                table: "DomainUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "Score",
                table: "Attempts",
                type: "numeric(9,2)",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "numeric(5,2)");

            migrationBuilder.AlterColumn<float>(
                name: "Score",
                table: "AttemptQuestions",
                type: "numeric(9,2)",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "numeric(5,2)");

            migrationBuilder.AddColumn<string>(
                name: "BanReason",
                table: "AspNetUsers",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Media",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ObjectKey = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ThumbnailKey = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: false),
                    Height = table.Column<int>(type: "integer", nullable: false),
                    DurationSeconds = table.Column<double>(type: "double precision", nullable: true),
                    OriginalFileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ModerationStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ModerationResultJson = table.Column<string>(type: "json", nullable: true),
                    ModeratedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ModerationAttempts = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Media", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Media_DomainUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "DomainUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Media_DomainUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "DomainUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserViolations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    MediaId = table.Column<Guid>(type: "uuid", nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DetailJson = table.Column<string>(type: "json", nullable: true),
                    DeletedQuestionCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsExpired = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserViolations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserViolations_DomainUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "DomainUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestGrades_UserId_TestId",
                table: "TestGrades",
                columns: new[] { "UserId", "TestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_MediaId",
                table: "Questions",
                column: "MediaId");

            migrationBuilder.CreateIndex(
                name: "IX_AttemptQuestions_AttemptId_QuestionId",
                table: "AttemptQuestions",
                columns: new[] { "AttemptId", "QuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Media_CreatedBy",
                table: "Media",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Media_OwnerId",
                table: "Media",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Media_Type_ModerationStatus",
                table: "Media",
                columns: new[] { "Type", "ModerationStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_UserViolations_UserId_IsExpired_ExpiresAt",
                table: "UserViolations",
                columns: new[] { "UserId", "IsExpired", "ExpiresAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Media_MediaId",
                table: "Questions",
                column: "MediaId",
                principalTable: "Media",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Media_MediaId",
                table: "Questions");

            migrationBuilder.DropTable(
                name: "Media");

            migrationBuilder.DropTable(
                name: "UserViolations");

            migrationBuilder.DropIndex(
                name: "IX_TestGrades_UserId_TestId",
                table: "TestGrades");

            migrationBuilder.DropIndex(
                name: "IX_Questions_MediaId",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_AttemptQuestions_AttemptId_QuestionId",
                table: "AttemptQuestions");

            migrationBuilder.DropColumn(
                name: "MediaId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "MediaType",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "CanUploadImage",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "CanUploadVideo",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "BanReason",
                table: "DomainUsers");

            migrationBuilder.DropColumn(
                name: "BannedAt",
                table: "DomainUsers");

            migrationBuilder.DropColumn(
                name: "BanReason",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<float>(
                name: "Score",
                table: "TestGrades",
                type: "numeric(5,2)",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "numeric(9,2)");

            migrationBuilder.AlterColumn<float>(
                name: "Score",
                table: "Attempts",
                type: "numeric(5,2)",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "numeric(9,2)");

            migrationBuilder.AlterColumn<float>(
                name: "Score",
                table: "AttemptQuestions",
                type: "numeric(5,2)",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "numeric(9,2)");

            migrationBuilder.CreateIndex(
                name: "IX_TestGrades_UserId",
                table: "TestGrades",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AttemptQuestions_AttemptId",
                table: "AttemptQuestions",
                column: "AttemptId");
        }
    }
}
