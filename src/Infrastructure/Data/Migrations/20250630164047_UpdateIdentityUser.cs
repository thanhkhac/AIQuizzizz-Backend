using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitectureBase.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIdentityUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailVerificationCode",
                table: "AspNetUsers",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailVerificationCodeTime",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmailVerificationLockout",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PasswordResetCode",
                table: "AspNetUsers",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetCodeExpiryTime",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PasswordResetLockout",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailVerificationCode",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmailVerificationCodeTime",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmailVerificationLockout",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PasswordResetCode",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PasswordResetCodeExpiryTime",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PasswordResetLockout",
                table: "AspNetUsers");
        }
    }
}
