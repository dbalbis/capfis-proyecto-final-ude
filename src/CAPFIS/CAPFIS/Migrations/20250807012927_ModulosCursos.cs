using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAPFIS.Migrations
{
    /// <inheritdoc />
    public partial class ModulosCursos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Contenido",
                table: "Etapas");

            migrationBuilder.AddColumn<string>(
                name: "ContenidoJson",
                table: "Etapas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContenidoTexto",
                table: "Etapas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContenidoUrl",
                table: "Etapas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EstaPublicado",
                table: "Etapas",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContenidoJson",
                table: "Etapas");

            migrationBuilder.DropColumn(
                name: "ContenidoTexto",
                table: "Etapas");

            migrationBuilder.DropColumn(
                name: "ContenidoUrl",
                table: "Etapas");

            migrationBuilder.DropColumn(
                name: "EstaPublicado",
                table: "Etapas");

            migrationBuilder.AddColumn<string>(
                name: "Contenido",
                table: "Etapas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
