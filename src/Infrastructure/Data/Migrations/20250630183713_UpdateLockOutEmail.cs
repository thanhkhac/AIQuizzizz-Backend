using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitectureBase.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLockOutEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AddColumn<int>(
                name: "EmailRequestLockout",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailRequestLockoutTime",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "EmailRequestLockout",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmailRequestLockoutTime",
                table: "AspNetUsers");
        }
    }
}
