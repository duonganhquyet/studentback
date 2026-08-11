using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentAcademicManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentFacultyMajorClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FacultyName",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MajorName",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ClassName",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FacultyName",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "MajorName",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ClassName",
                table: "Students");
        }
    }
}
