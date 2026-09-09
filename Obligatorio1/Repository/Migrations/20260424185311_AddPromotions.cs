using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    [ExcludeFromCodeCoverage]
    /// <inheritdoc />
    public partial class AddPromotions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Promotions_CommercialLines_CommercialLineId",
                table: "Promotions");

            migrationBuilder.AddForeignKey(
                name: "FK_Promotions_CommercialLines_CommercialLineId",
                table: "Promotions",
                column: "CommercialLineId",
                principalTable: "CommercialLines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Promotions_CommercialLines_CommercialLineId",
                table: "Promotions");

            migrationBuilder.AddForeignKey(
                name: "FK_Promotions_CommercialLines_CommercialLineId",
                table: "Promotions",
                column: "CommercialLineId",
                principalTable: "CommercialLines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
