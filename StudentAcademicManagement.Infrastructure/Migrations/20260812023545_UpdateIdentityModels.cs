using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentAcademicManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIdentityModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RegionType",
                table: "StudentProfiles",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegionType",
                table: "StudentProfiles");
        }
    }
}
