using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetSentinel.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddHashApplicationColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HashApplication",
                table: "tb_installed_applications",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HashApplication",
                table: "tb_installed_applications");
        }
    }
}
