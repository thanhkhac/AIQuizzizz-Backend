using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitectureBase.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNavigations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "Score",
                table: "Questions",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(float),
                oldType: "numeric(5,2)");

            migrationBuilder.AlterColumn<string>(
                name: "ExplainText",
                table: "Questions",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "TokenCount",
                table: "DomainUsers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "DomainUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsBanned",
                table: "DomainUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "DomainUsers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "DomainUsers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "TestTemplateUsers",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TestTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShareMode = table.Column<string>(type: "text", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestTemplateUsers", x => new { x.UserId, x.TestTemplateId });
                    table.ForeignKey(
                        name: "FK_TestTemplateUsers_DomainUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "DomainUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TestTemplateUsers_DomainUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "DomainUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TestTemplateUsers_TestTemplates_TestTemplateId",
                        column: x => x.TestTemplateId,
                        principalTable: "TestTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserTokenPurchases_CreatedBy",
                table: "UserTokenPurchases",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserQuestionSetHistories_CreatedBy",
                table: "UserQuestionSetHistories",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TokenPackages_CreatedBy",
                table: "TokenPackages",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TodoLists_CreatedBy",
                table: "TodoLists",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_CreatedBy",
                table: "TodoItems",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TestTemplates_CreatedBy",
                table: "TestTemplates",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TestTemplateQuestions_CreatedBy",
                table: "TestTemplateQuestions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Tests_CreatedBy",
                table: "Tests",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSetUsers_CreatedBy",
                table: "QuestionSetUsers",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSets_CreatedBy",
                table: "QuestionSets",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_CreatedBy",
                table: "Questions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Plans_CreatedBy",
                table: "Plans",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_FolderUsers_CreatedBy",
                table: "FolderUsers",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_FolderTestTemplates_CreatedBy",
                table: "FolderTestTemplates",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Folders_CreatedBy",
                table: "Folders",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_CreatedBy",
                table: "Comments",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ClassUsers_CreatedBy",
                table: "ClassUsers",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ClassQuestionSets_CreatedBy",
                table: "ClassQuestionSets",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ClassInvitations_CreatedBy",
                table: "ClassInvitations",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_CreatedBy",
                table: "Classes",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Attempts_TestVersionId",
                table: "Attempts",
                column: "TestVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_TestTemplateUsers_CreatedBy",
                table: "TestTemplateUsers",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TestTemplateUsers_TestTemplateId",
                table: "TestTemplateUsers",
                column: "TestTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attempts_TestVersions_TestVersionId",
                table: "Attempts",
                column: "TestVersionId",
                principalTable: "TestVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_DomainUsers_CreatedBy",
                table: "Classes",
                column: "CreatedBy",
                principalTable: "DomainUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassInvitations_DomainUsers_CreatedBy",
                table: "ClassInvitations",
                column: "CreatedBy",
                principalTable: "DomainUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassQuestionSets_DomainUsers_CreatedBy",
                table: "ClassQuestionSets",
                column: "CreatedBy",
                principalTable: "DomainUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassUsers_DomainUsers_CreatedBy",
                table: "ClassUsers",
                column: "CreatedBy",
                principalTable: "DomainUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_DomainUsers_CreatedBy",
                table: "Comments",
                column: "CreatedBy",
                principalTable: "DomainUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Folders_DomainUsers_CreatedBy",
                table: "Folders",
                column: "CreatedBy",
                principalTable: "DomainUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FolderTestTemplates_DomainUsers_CreatedBy",
                table: "FolderTestTemplates",
                column: "CreatedBy",
                principalTable: "DomainUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FolderUsers_DomainUsers_CreatedBy",
                table: "FolderUsers",
                column: "CreatedBy",
                principalTable: "DomainUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Plans_DomainUsers_CreatedBy",
                table: "Plans",
                column: "CreatedBy",
                principalTable: "DomainUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_DomainUsers_CreatedBy",
                table: "Questions",
                column: "CreatedBy",
                principalTable: "DomainUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionSets_DomainUsers_CreatedBy",
                table: "QuestionSets",
                column: "CreatedBy",
                principalTable: "DomainUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionSetUsers_DomainUsers_CreatedBy",
                table: "QuestionSetUsers",
                column: "CreatedBy",
                principalTable: "DomainUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tests_DomainUsers_CreatedBy",
                table: "Tests",
                column: "CreatedBy",
                principalTable: "DomainUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TestTemplateQuestions_DomainUsers_CreatedBy",
                table: "TestTemplateQuestions",
                column: "CreatedBy",
                principalTable: "DomainUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TestTemplates_DomainUsers_CreatedBy",
                table: "TestTemplates",
                column: "CreatedBy",
                principalTable: "DomainUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TodoItems_DomainUsers_CreatedBy",
                table: "TodoItems",
                column: "CreatedBy",
                principalTable: "DomainUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TodoLists_DomainUsers_CreatedBy",
                table: "TodoLists",
                column: "CreatedBy",
                principalTable: "DomainUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TokenPackages_DomainUsers_CreatedBy",
                table: "TokenPackages",
                column: "CreatedBy",
                principalTable: "DomainUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserQuestionSetHistories_DomainUsers_CreatedBy",
                table: "UserQuestionSetHistories",
                column: "CreatedBy",
                principalTable: "DomainUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTokenPurchases_DomainUsers_CreatedBy",
                table: "UserTokenPurchases",
                column: "CreatedBy",
                principalTable: "DomainUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attempts_TestVersions_TestVersionId",
                table: "Attempts");

            migrationBuilder.DropForeignKey(
                name: "FK_Classes_DomainUsers_CreatedBy",
                table: "Classes");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassInvitations_DomainUsers_CreatedBy",
                table: "ClassInvitations");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassQuestionSets_DomainUsers_CreatedBy",
                table: "ClassQuestionSets");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassUsers_DomainUsers_CreatedBy",
                table: "ClassUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_DomainUsers_CreatedBy",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Folders_DomainUsers_CreatedBy",
                table: "Folders");

            migrationBuilder.DropForeignKey(
                name: "FK_FolderTestTemplates_DomainUsers_CreatedBy",
                table: "FolderTestTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_FolderUsers_DomainUsers_CreatedBy",
                table: "FolderUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Plans_DomainUsers_CreatedBy",
                table: "Plans");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_DomainUsers_CreatedBy",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestionSets_DomainUsers_CreatedBy",
                table: "QuestionSets");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestionSetUsers_DomainUsers_CreatedBy",
                table: "QuestionSetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Tests_DomainUsers_CreatedBy",
                table: "Tests");

            migrationBuilder.DropForeignKey(
                name: "FK_TestTemplateQuestions_DomainUsers_CreatedBy",
                table: "TestTemplateQuestions");

            migrationBuilder.DropForeignKey(
                name: "FK_TestTemplates_DomainUsers_CreatedBy",
                table: "TestTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_TodoItems_DomainUsers_CreatedBy",
                table: "TodoItems");

            migrationBuilder.DropForeignKey(
                name: "FK_TodoLists_DomainUsers_CreatedBy",
                table: "TodoLists");

            migrationBuilder.DropForeignKey(
                name: "FK_TokenPackages_DomainUsers_CreatedBy",
                table: "TokenPackages");

            migrationBuilder.DropForeignKey(
                name: "FK_UserQuestionSetHistories_DomainUsers_CreatedBy",
                table: "UserQuestionSetHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTokenPurchases_DomainUsers_CreatedBy",
                table: "UserTokenPurchases");

            migrationBuilder.DropTable(
                name: "TestTemplateUsers");

            migrationBuilder.DropIndex(
                name: "IX_UserTokenPurchases_CreatedBy",
                table: "UserTokenPurchases");

            migrationBuilder.DropIndex(
                name: "IX_UserQuestionSetHistories_CreatedBy",
                table: "UserQuestionSetHistories");

            migrationBuilder.DropIndex(
                name: "IX_TokenPackages_CreatedBy",
                table: "TokenPackages");

            migrationBuilder.DropIndex(
                name: "IX_TodoLists_CreatedBy",
                table: "TodoLists");

            migrationBuilder.DropIndex(
                name: "IX_TodoItems_CreatedBy",
                table: "TodoItems");

            migrationBuilder.DropIndex(
                name: "IX_TestTemplates_CreatedBy",
                table: "TestTemplates");

            migrationBuilder.DropIndex(
                name: "IX_TestTemplateQuestions_CreatedBy",
                table: "TestTemplateQuestions");

            migrationBuilder.DropIndex(
                name: "IX_Tests_CreatedBy",
                table: "Tests");

            migrationBuilder.DropIndex(
                name: "IX_QuestionSetUsers_CreatedBy",
                table: "QuestionSetUsers");

            migrationBuilder.DropIndex(
                name: "IX_QuestionSets_CreatedBy",
                table: "QuestionSets");

            migrationBuilder.DropIndex(
                name: "IX_Questions_CreatedBy",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Plans_CreatedBy",
                table: "Plans");

            migrationBuilder.DropIndex(
                name: "IX_FolderUsers_CreatedBy",
                table: "FolderUsers");

            migrationBuilder.DropIndex(
                name: "IX_FolderTestTemplates_CreatedBy",
                table: "FolderTestTemplates");

            migrationBuilder.DropIndex(
                name: "IX_Folders_CreatedBy",
                table: "Folders");

            migrationBuilder.DropIndex(
                name: "IX_Comments_CreatedBy",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_ClassUsers_CreatedBy",
                table: "ClassUsers");

            migrationBuilder.DropIndex(
                name: "IX_ClassQuestionSets_CreatedBy",
                table: "ClassQuestionSets");

            migrationBuilder.DropIndex(
                name: "IX_ClassInvitations_CreatedBy",
                table: "ClassInvitations");

            migrationBuilder.DropIndex(
                name: "IX_Classes_CreatedBy",
                table: "Classes");

            migrationBuilder.DropIndex(
                name: "IX_Attempts_TestVersionId",
                table: "Attempts");

            migrationBuilder.AlterColumn<float>(
                name: "Score",
                table: "Questions",
                type: "numeric(5,2)",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "numeric(5,2)",
                oldDefaultValue: 0f);

            migrationBuilder.AlterColumn<string>(
                name: "ExplainText",
                table: "Questions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "TokenCount",
                table: "DomainUsers",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldDefaultValue: 0L);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "DomainUsers",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsBanned",
                table: "DomainUsers",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "DomainUsers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "DomainUsers",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);
        }
    }
}
