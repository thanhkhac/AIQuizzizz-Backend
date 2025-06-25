using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitectureBase.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClassrelationtoTestentityandconfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClassId",
                table: "Tests",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Tests_ClassId",
                table: "Tests",
                column: "ClassId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tests_Classes_ClassId",
                table: "Tests",
                column: "ClassId",
                principalTable: "Classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tests_Classes_ClassId",
                table: "Tests");

            migrationBuilder.DropIndex(
                name: "IX_Tests_ClassId",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "ClassId",
                table: "Tests");
        }
    }
}
