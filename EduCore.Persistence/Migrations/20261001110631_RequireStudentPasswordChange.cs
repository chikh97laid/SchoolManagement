using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduCore.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RequireStudentPasswordChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "MustChangePassword",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(
                "UPDATE \"AspNetUsers\" SET \"MustChangePassword\" = TRUE " +
                "WHERE \"Id\" IN (SELECT \"UserId\" FROM \"Students\" WHERE \"UserId\" IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MustChangePassword",
                table: "AspNetUsers");
        }
    }
}
