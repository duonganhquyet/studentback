using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentAcademicManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAcademicHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AwardDetails",
                table: "StudentProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentRoleInSchool",
                table: "StudentProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasBeenClassMonitor",
                table: "StudentProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasBeenYouthUnionOfficer",
                table: "StudentProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasParticipatedInExcellentStudentTeam",
                table: "StudentProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "StudentAcademicHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    FromTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SchoolName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentAcademicHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentAcademicHistories_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentAcademicHistories_StudentId",
                table: "StudentAcademicHistories",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentAcademicHistories");

            migrationBuilder.DropColumn(
                name: "AwardDetails",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "CurrentRoleInSchool",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "HasBeenClassMonitor",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "HasBeenYouthUnionOfficer",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "HasParticipatedInExcellentStudentTeam",
                table: "StudentProfiles");
        }
    }
}
