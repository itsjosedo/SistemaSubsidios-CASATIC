using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaSubsidios.Migrations
{
    /// <inheritdoc />
    public partial class ActualizacionDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UsuarioId_Usuario",
                table: "Beneficiarios",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiarios_UsuarioId_Usuario",
                table: "Beneficiarios",
                column: "UsuarioId_Usuario");

            migrationBuilder.AddForeignKey(
                name: "FK_Beneficiarios_Usuarios_UsuarioId_Usuario",
                table: "Beneficiarios",
                column: "UsuarioId_Usuario",
                principalTable: "Usuarios",
                principalColumn: "Id_Usuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Beneficiarios_Usuarios_UsuarioId_Usuario",
                table: "Beneficiarios");

            migrationBuilder.DropIndex(
                name: "IX_Beneficiarios_UsuarioId_Usuario",
                table: "Beneficiarios");

            migrationBuilder.DropColumn(
                name: "UsuarioId_Usuario",
                table: "Beneficiarios");
        }
    }
}
