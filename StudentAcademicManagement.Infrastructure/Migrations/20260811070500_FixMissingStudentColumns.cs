using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentAcademicManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixMissingStudentColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Students]') AND name = 'FacultyName')
                BEGIN
                    ALTER TABLE [Students] ADD [FacultyName] nvarchar(max) NOT NULL DEFAULT N'';
                END

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Students]') AND name = 'MajorName')
                BEGIN
                    ALTER TABLE [Students] ADD [MajorName] nvarchar(max) NOT NULL DEFAULT N'';
                END

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Students]') AND name = 'ClassName')
                BEGIN
                    ALTER TABLE [Students] ADD [ClassName] nvarchar(max) NOT NULL DEFAULT N'';
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
