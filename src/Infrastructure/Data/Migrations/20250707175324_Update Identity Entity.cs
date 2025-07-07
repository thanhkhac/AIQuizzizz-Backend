using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitectureBase.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIdentityEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailRequestLockout",
                table: "DomainUsers");

            migrationBuilder.DropColumn(
                name: "EmailRequestLockoutTime",
                table: "DomainUsers");

            migrationBuilder.DropColumn(
                name: "EmailVerificationCode",
                table: "DomainUsers");

            migrationBuilder.DropColumn(
                name: "EmailVerificationCodeTime",
                table: "DomainUsers");

            migrationBuilder.DropColumn(
                name: "EmailVerificationLockout",
                table: "DomainUsers");

            migrationBuilder.DropColumn(
                name: "PasswordResetCode",
                table: "DomainUsers");

            migrationBuilder.DropColumn(
                name: "PasswordResetCodeExpiryTime",
                table: "DomainUsers");

            migrationBuilder.DropColumn(
                name: "PasswordResetLockout",
                table: "DomainUsers");

            migrationBuilder.RenameColumn(
                name: "PasswordResetLockout",
                table: "AspNetUsers",
                newName: "PasswordResetRequestAttempts");

            migrationBuilder.RenameColumn(
                name: "EmailVerificationLockout",
                table: "AspNetUsers",
                newName: "FailedPasswordResetAttempts");

            migrationBuilder.RenameColumn(
                name: "EmailVerificationCodeTime",
                table: "AspNetUsers",
                newName: "PasswordResetRequestLockoutEnd");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EmailVerificationCodeExpiryTime",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EmailVerificationLockoutEnd",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmailVerificationRequestAttempts",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EmailVerificationRequestLockoutEnd",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FailedEmailVerificationAttempts",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PasswordResetLockoutEnd",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailVerificationCodeExpiryTime",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmailVerificationLockoutEnd",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmailVerificationRequestAttempts",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmailVerificationRequestLockoutEnd",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FailedEmailVerificationAttempts",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PasswordResetLockoutEnd",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "PasswordResetRequestLockoutEnd",
                table: "AspNetUsers",
                newName: "EmailVerificationCodeTime");

            migrationBuilder.RenameColumn(
                name: "PasswordResetRequestAttempts",
                table: "AspNetUsers",
                newName: "PasswordResetLockout");

            migrationBuilder.RenameColumn(
                name: "FailedPasswordResetAttempts",
                table: "AspNetUsers",
                newName: "EmailVerificationLockout");

            migrationBuilder.AddColumn<int>(
                name: "EmailRequestLockout",
                table: "DomainUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailRequestLockoutTime",
                table: "DomainUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailVerificationCode",
                table: "DomainUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailVerificationCodeTime",
                table: "DomainUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmailVerificationLockout",
                table: "DomainUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PasswordResetCode",
                table: "DomainUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetCodeExpiryTime",
                table: "DomainUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PasswordResetLockout",
                table: "DomainUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
