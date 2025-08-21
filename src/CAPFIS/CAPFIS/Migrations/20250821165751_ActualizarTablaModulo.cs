using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAPFIS.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarTablaModulo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BotonTexto",
                table: "Modulos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BotonUrl",
                table: "Modulos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagenHero",
                table: "Modulos",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BotonTexto",
                table: "Modulos");

            migrationBuilder.DropColumn(
                name: "BotonUrl",
                table: "Modulos");

            migrationBuilder.DropColumn(
                name: "ImagenHero",
                table: "Modulos");
        }
    }
}
