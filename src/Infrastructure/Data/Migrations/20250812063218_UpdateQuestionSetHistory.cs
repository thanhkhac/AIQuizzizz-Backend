using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitectureBase.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQuestionSetHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserQuestionSetHistories",
                table: "UserQuestionSetHistories");

            migrationBuilder.DropIndex(
                name: "IX_UserQuestionSetHistories_UserId",
                table: "UserQuestionSetHistories");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "UserQuestionSetHistories");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserQuestionSetHistories",
                table: "UserQuestionSetHistories",
                columns: new[] { "UserId", "QuestionId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserQuestionSetHistories",
                table: "UserQuestionSetHistories");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "UserQuestionSetHistories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserQuestionSetHistories",
                table: "UserQuestionSetHistories",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserQuestionSetHistories_UserId",
                table: "UserQuestionSetHistories",
                column: "UserId");
        }
    }
}
