using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitectureBase.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Updatetimelimitdatatype : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeLimit",
                table: "Tests");

            migrationBuilder.AddColumn<int>(
                name: "TimeLimit",
                table: "Tests",
                type: "integer",
                nullable: false,
                defaultValue: 0); 
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeLimit",
                table: "Tests");

            migrationBuilder.AddColumn<DateTime>(
                name: "TimeLimit",
                table: "Tests",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: DateTime.UtcNow); 
        }
    }
}
