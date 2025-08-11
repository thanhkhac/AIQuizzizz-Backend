using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitectureBase.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDataBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserTokenPurchases");

            migrationBuilder.DropTable(
                name: "TokenPackages");

            migrationBuilder.AddColumn<bool>(
                name: "IsPaymentLocked",
                table: "DomainUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "QuestionSetRatings",
                columns: table => new
                {
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionSetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionSetRatings", x => new { x.QuestionSetId, x.CreatedBy });
                    table.ForeignKey(
                        name: "FK_QuestionSetRatings_DomainUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "DomainUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuestionSetRatings_QuestionSets_QuestionSetId",
                        column: x => x.QuestionSetId,
                        principalTable: "QuestionSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSetRatings_CreatedBy",
                table: "QuestionSetRatings",
                column: "CreatedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestionSetRatings");

            migrationBuilder.DropColumn(
                name: "IsPaymentLocked",
                table: "DomainUsers");

            migrationBuilder.CreateTable(
                name: "TokenPackages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TokenCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenPackages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TokenPackages_DomainUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "DomainUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserTokenPurchases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    TokenPackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTokenPurchases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTokenPurchases_DomainUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "DomainUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserTokenPurchases_DomainUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "DomainUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserTokenPurchases_TokenPackages_TokenPackageId",
                        column: x => x.TokenPackageId,
                        principalTable: "TokenPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TokenPackages_CreatedBy",
                table: "TokenPackages",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserTokenPurchases_CreatedBy",
                table: "UserTokenPurchases",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserTokenPurchases_TokenPackageId",
                table: "UserTokenPurchases",
                column: "TokenPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTokenPurchases_UserId",
                table: "UserTokenPurchases",
                column: "UserId");
        }
    }
}
