using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Proejkt_magazyn.Migrations
{
    /// <inheritdoc />
    public partial class RestrictProductDeleteAndAddEanIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PozycjeZamowien_Produkty_ProduktId",
                table: "PozycjeZamowien");

            migrationBuilder.CreateIndex(
                name: "IX_Produkty_KodKreskowy",
                table: "Produkty",
                column: "KodKreskowy",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PozycjeZamowien_Produkty_ProduktId",
                table: "PozycjeZamowien",
                column: "ProduktId",
                principalTable: "Produkty",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PozycjeZamowien_Produkty_ProduktId",
                table: "PozycjeZamowien");

            migrationBuilder.DropIndex(
                name: "IX_Produkty_KodKreskowy",
                table: "Produkty");

            migrationBuilder.AddForeignKey(
                name: "FK_PozycjeZamowien_Produkty_ProduktId",
                table: "PozycjeZamowien",
                column: "ProduktId",
                principalTable: "Produkty",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
