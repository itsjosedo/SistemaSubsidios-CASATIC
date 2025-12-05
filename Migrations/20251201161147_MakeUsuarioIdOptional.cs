using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaSubsidios.Migrations
{
    /// <inheritdoc />
    public partial class MakeUsuarioIdOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Beneficiarios_Usuarios_UsuarioId",
                table: "Beneficiarios");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioId",
                table: "Beneficiarios",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Beneficiarios_Usuarios_UsuarioId",
                table: "Beneficiarios",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id_Usuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Beneficiarios_Usuarios_UsuarioId",
                table: "Beneficiarios");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioId",
                table: "Beneficiarios",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Beneficiarios_Usuarios_UsuarioId",
                table: "Beneficiarios",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id_Usuario",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
