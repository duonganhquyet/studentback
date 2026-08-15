using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentAcademicManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddResidenceInfoToContact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LandlordName",
                table: "StudentContacts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LandlordPhone",
                table: "StudentContacts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResidenceType",
                table: "StudentContacts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LandlordName",
                table: "StudentContacts");

            migrationBuilder.DropColumn(
                name: "LandlordPhone",
                table: "StudentContacts");

            migrationBuilder.DropColumn(
                name: "ResidenceType",
                table: "StudentContacts");
        }
    }
}
