using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitectureBase.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTagEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuestionSetCount",
                table: "Tags",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuestionSetCount",
                table: "Tags");
        }
    }
}
