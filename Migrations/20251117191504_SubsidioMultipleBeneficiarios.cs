using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaSubsidios.Migrations
{
    /// <inheritdoc />
    public partial class SubsidioMultipleBeneficiarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subsidios_Beneficiarios_BeneficiarioId",
                table: "Subsidios");

            migrationBuilder.DropIndex(
                name: "IX_Subsidios_BeneficiarioId",
                table: "Subsidios");

            migrationBuilder.DropColumn(
                name: "BeneficiarioId",
                table: "Subsidios");

            migrationBuilder.CreateTable(
                name: "SubsidioBeneficiario",
                columns: table => new
                {
                    BeneficiariosId_Beneficiario = table.Column<int>(type: "int", nullable: false),
                    SubsidiosId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubsidioBeneficiario", x => new { x.BeneficiariosId_Beneficiario, x.SubsidiosId });
                    table.ForeignKey(
                        name: "FK_SubsidioBeneficiario_Beneficiarios_BeneficiariosId_Beneficia~",
                        column: x => x.BeneficiariosId_Beneficiario,
                        principalTable: "Beneficiarios",
                        principalColumn: "Id_Beneficiario",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubsidioBeneficiario_Subsidios_SubsidiosId",
                        column: x => x.SubsidiosId,
                        principalTable: "Subsidios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SubsidioBeneficiario_SubsidiosId",
                table: "SubsidioBeneficiario",
                column: "SubsidiosId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubsidioBeneficiario");

            migrationBuilder.AddColumn<int>(
                name: "BeneficiarioId",
                table: "Subsidios",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Subsidios_BeneficiarioId",
                table: "Subsidios",
                column: "BeneficiarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Subsidios_Beneficiarios_BeneficiarioId",
                table: "Subsidios",
                column: "BeneficiarioId",
                principalTable: "Beneficiarios",
                principalColumn: "Id_Beneficiario",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
