using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetSentinel.Api.Migrations
{
    /// <inheritdoc />
    public partial class DropInlineVulnColumnsFromSoftwareVulnerabilities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Description",     table: "tb_software_vulnerabilities");
            migrationBuilder.DropColumn(name: "CvssScore",       table: "tb_software_vulnerabilities");
            migrationBuilder.DropColumn(name: "Severity",        table: "tb_software_vulnerabilities");
            migrationBuilder.DropColumn(name: "ResolutionMode",  table: "tb_software_vulnerabilities");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "Description",    table: "tb_software_vulnerabilities", nullable: false, defaultValue: "");
            migrationBuilder.AddColumn<double>(name: "CvssScore",      table: "tb_software_vulnerabilities", nullable: false, defaultValue: 0.0);
            migrationBuilder.AddColumn<string>(name: "Severity",       table: "tb_software_vulnerabilities", nullable: false, defaultValue: "");
            migrationBuilder.AddColumn<string>(name: "ResolutionMode", table: "tb_software_vulnerabilities", nullable: false, defaultValue: "");
        }
    }
}
