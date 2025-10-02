using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaSubsidios.Migrations
{
    /// <inheritdoc />
    public partial class UsuarioIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entidades_Usuarios_UsuarioId",
                table: "Entidades");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioId",
                table: "Entidades",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Entidades_Usuarios_UsuarioId",
                table: "Entidades",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id_Usuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entidades_Usuarios_UsuarioId",
                table: "Entidades");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioId",
                table: "Entidades",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Entidades_Usuarios_UsuarioId",
                table: "Entidades",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id_Usuario",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
