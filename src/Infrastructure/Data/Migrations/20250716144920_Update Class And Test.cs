using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitectureBase.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateClassAndTest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "PassingScore",
                table: "Tests",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "Topic",
                table: "Classes",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PassingScore",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "Topic",
                table: "Classes");
        }
    }
}
