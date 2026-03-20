using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceWarehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRawMaterialToMatchExcel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RawMaterialId",
                table: "OutboundOrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RawMaterialId",
                table: "Inventories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RawMaterialId",
                table: "InboundOrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RawMaterial",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    MaterialName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Specification = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RemainingQuantity = table.Column<int>(type: "int", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaterialStatus = table.Column<int>(type: "int", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Supplier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawMaterial", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutboundOrderItems_RawMaterialId",
                table: "OutboundOrderItems",
                column: "RawMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_RawMaterialId",
                table: "Inventories",
                column: "RawMaterialId",
                unique: true,
                filter: "[RawMaterialId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InboundOrderItems_RawMaterialId",
                table: "InboundOrderItems",
                column: "RawMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_RawMaterial_MaterialName",
                table: "RawMaterial",
                column: "MaterialName");

            migrationBuilder.AddForeignKey(
                name: "FK_InboundOrderItems_RawMaterial_RawMaterialId",
                table: "InboundOrderItems",
                column: "RawMaterialId",
                principalTable: "RawMaterial",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_RawMaterial_RawMaterialId",
                table: "Inventories",
                column: "RawMaterialId",
                principalTable: "RawMaterial",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OutboundOrderItems_RawMaterial_RawMaterialId",
                table: "OutboundOrderItems",
                column: "RawMaterialId",
                principalTable: "RawMaterial",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InboundOrderItems_RawMaterial_RawMaterialId",
                table: "InboundOrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_RawMaterial_RawMaterialId",
                table: "Inventories");

            migrationBuilder.DropForeignKey(
                name: "FK_OutboundOrderItems_RawMaterial_RawMaterialId",
                table: "OutboundOrderItems");

            migrationBuilder.DropTable(
                name: "RawMaterial");

            migrationBuilder.DropIndex(
                name: "IX_OutboundOrderItems_RawMaterialId",
                table: "OutboundOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_RawMaterialId",
                table: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_InboundOrderItems_RawMaterialId",
                table: "InboundOrderItems");

            migrationBuilder.DropColumn(
                name: "RawMaterialId",
                table: "OutboundOrderItems");

            migrationBuilder.DropColumn(
                name: "RawMaterialId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "RawMaterialId",
                table: "InboundOrderItems");
        }
    }
}
