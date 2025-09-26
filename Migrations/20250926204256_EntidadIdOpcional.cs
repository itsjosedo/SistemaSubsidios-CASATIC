using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaSubsidios.Migrations
{
    /// <inheritdoc />
    public partial class EntidadIdOpcional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Beneficiarios_Entidades_EntidadId",
                table: "Beneficiarios");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Entidades");

            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "Beneficiarios");

            migrationBuilder.DropColumn(
                name: "FechaModificacion",
                table: "Beneficiarios");

            migrationBuilder.DropColumn(
                name: "UsuarioCreacion",
                table: "Beneficiarios");

            migrationBuilder.DropColumn(
                name: "UsuarioModificacion",
                table: "Beneficiarios");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Entidades",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "EntidadId",
                table: "Beneficiarios",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Beneficiarios_Entidades_EntidadId",
                table: "Beneficiarios",
                column: "EntidadId",
                principalTable: "Entidades",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Beneficiarios_Entidades_EntidadId",
                table: "Beneficiarios");

            migrationBuilder.UpdateData(
                table: "Entidades",
                keyColumn: "Nombre",
                keyValue: null,
                column: "Nombre",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Entidades",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Entidades",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "EntidadId",
                table: "Beneficiarios",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "Beneficiarios",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaModificacion",
                table: "Beneficiarios",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsuarioCreacion",
                table: "Beneficiarios",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "UsuarioModificacion",
                table: "Beneficiarios",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_Beneficiarios_Entidades_EntidadId",
                table: "Beneficiarios",
                column: "EntidadId",
                principalTable: "Entidades",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
