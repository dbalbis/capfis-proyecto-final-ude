using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAPFIS.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTituloDetalladoModulo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TituloDetallado",
                table: "Modulos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TituloDetallado",
                table: "Modulos");
        }
    }
}
