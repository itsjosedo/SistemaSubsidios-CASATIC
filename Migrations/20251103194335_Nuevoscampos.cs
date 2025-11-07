using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaSubsidios.Migrations
{
    /// <inheritdoc />
    public partial class Nuevoscampos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaNacimiento",
                table: "Beneficiarios",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Genero",
                table: "Beneficiarios",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaNacimiento",
                table: "Beneficiarios");

            migrationBuilder.DropColumn(
                name: "Genero",
                table: "Beneficiarios");
        }
    }
}
