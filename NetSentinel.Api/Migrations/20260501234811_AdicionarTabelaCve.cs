using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetSentinel.Api.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarTabelaCve : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Pulando colunas que já foram removidas manualmente ou em tentativas anteriores
            /*
            migrationBuilder.DropColumn(
                name: "CvssScore",
                table: "tb_software_vulnerabilities");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "tb_software_vulnerabilities");

            migrationBuilder.DropColumn(
                name: "ResolutionMode",
                table: "tb_software_vulnerabilities");

            migrationBuilder.DropColumn(
                name: "Severity",
                table: "tb_software_vulnerabilities");
            */

            migrationBuilder.AlterColumn<int>(
                name: "CveId",
                table: "tb_software_vulnerabilities",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            // 2. Comentado porque o log acusou que 'HashApplication' já existe no banco físico
            /*
            migrationBuilder.AddColumn<string>(
                name: "HashApplication",
                table: "tb_installed_applications",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
            */

            // 3. Criação da tabela de CVEs (Necessário para o VulnerabilityScannerWorker)
            migrationBuilder.CreateTable(
                name: "tb_cves",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CveName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CvssScore = table.Column<double>(type: "double", nullable: false),
                    Severity = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ResolutionMode = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_cves", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_tb_software_vulnerabilities_CveId",
                table: "tb_software_vulnerabilities",
                column: "CveId");

            migrationBuilder.AddForeignKey(
                name: "FK_tb_software_vulnerabilities_tb_cves_CveId",
                table: "tb_software_vulnerabilities",
                column: "CveId",
                principalTable: "tb_cves",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_software_vulnerabilities_tb_cves_CveId",
                table: "tb_software_vulnerabilities");

            migrationBuilder.DropTable(
                name: "tb_cves");

            migrationBuilder.DropIndex(
                name: "IX_tb_software_vulnerabilities_CveId",
                table: "tb_software_vulnerabilities");

            migrationBuilder.DropColumn(
                name: "HashApplication",
                table: "tb_installed_applications");

            migrationBuilder.UpdateData(
                table: "tb_software_vulnerabilities",
                keyColumn: "CveId",
                keyValue: null,
                column: "CveId",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "CveId",
                table: "tb_software_vulnerabilities",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<double>(
                name: "CvssScore",
                table: "tb_software_vulnerabilities",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "tb_software_vulnerabilities",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ResolutionMode",
                table: "tb_software_vulnerabilities",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Severity",
                table: "tb_software_vulnerabilities",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}