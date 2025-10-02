using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaSubsidios.Migrations
{
    /// <inheritdoc />
    public partial class ModificarTablaEntidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Contacto",
                table: "Entidades",
                newName: "Email");

            migrationBuilder.AddColumn<string>(
                name: "Direccion",
                table: "Entidades",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Direccion",
                table: "Entidades");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Entidades",
                newName: "Contacto");
        }
    }
}
