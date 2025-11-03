using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HFile.Migrations
{
    /// <inheritdoc />
    public partial class MedicalFileUpload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Imageprofile",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Imageprofile",
                table: "Users");
        }
    }
}
