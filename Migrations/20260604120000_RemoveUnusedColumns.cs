using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Proejkt_magazyn.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CzyChemia",
                table: "Produkty");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Zamowienia");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CzyChemia",
                table: "Produkty",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Zamowienia",
                type: "TEXT",
                nullable: false,
                defaultValue: "Nowe");
        }
    }
}
