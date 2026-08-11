using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentAcademicManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSuperAdminPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$12$LeZohjv5cBvtODrGfDy7YOW0/vNNeOHoY251SLJSfDRdUQw8JpHzO");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$q5MhJWTsNcb9Q/uD0k8P.uE.b9.MIfE.lWbT.kF7n8Kqj/W0.hNCO");
        }
    }
}
