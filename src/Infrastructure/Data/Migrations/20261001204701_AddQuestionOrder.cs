using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitectureBase.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Questions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Backfill xác định: câu hỏi trong bộ câu hỏi theo (Created, Id)
            migrationBuilder.Sql(@"
UPDATE ""Questions"" q
SET ""Order"" = r.rn
FROM (
    SELECT ""Id"", ROW_NUMBER() OVER (PARTITION BY ""QuestionSetId"" ORDER BY ""Created"", ""Id"") - 1 AS rn
    FROM ""Questions""
    WHERE ""QuestionSetId"" IS NOT NULL
) r
WHERE q.""Id"" = r.""Id"";");

            // Câu hỏi thuộc test template (không có QuestionSetId): theo thứ tự thêm vào template
            migrationBuilder.Sql(@"
UPDATE ""Questions"" q
SET ""Order"" = r.rn
FROM (
    SELECT ""QuestionId"" AS ""Id"",
           ROW_NUMBER() OVER (PARTITION BY ""TestTemplateId"" ORDER BY ""Created"", ""Id"") - 1 AS rn
    FROM ""TestTemplateQuestions""
) r
WHERE q.""Id"" = r.""Id"" AND q.""QuestionSetId"" IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                table: "Questions");
        }
    }
}
