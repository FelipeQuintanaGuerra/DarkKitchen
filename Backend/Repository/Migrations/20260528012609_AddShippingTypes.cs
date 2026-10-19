using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Repository.Migrations
{
    /// <inheritdoc />
    [ExcludeFromCodeCoverage]

    public partial class AddShippingTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippingTypeName",
                table: "Orders");

            migrationBuilder.AddColumn<Guid>(
                name: "ShippingTypeId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.CreateTable(
                name: "ShippingTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingTypes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ShippingTypes",
                columns: new[] { "Id", "Cost", "Name" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), 250m, "Express" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), 180m, "24Hours" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ShippingTypeId",
                table: "Orders",
                column: "ShippingTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingTypes_Name",
                table: "ShippingTypes",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_ShippingTypes_ShippingTypeId",
                table: "Orders",
                column: "ShippingTypeId",
                principalTable: "ShippingTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_ShippingTypes_ShippingTypeId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "ShippingTypes");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ShippingTypeId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingTypeId",
                table: "Orders");

            migrationBuilder.AddColumn<string>(
                name: "ShippingTypeName",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
