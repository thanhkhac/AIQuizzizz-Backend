using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitectureBase.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class NormalizePlanUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Seed cũ dùng "day"/"month"/"year" trong khi validator + UI dùng "Day"/"Month"/"Year" -> admin không sửa được gói
            migrationBuilder.Sql("UPDATE \"Plans\" SET \"Unit\" = initcap(lower(\"Unit\")) WHERE lower(\"Unit\") IN ('day', 'month', 'year');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // không cần hoàn tác (chỉ chuẩn hoá dữ liệu)
        }
    }
}
