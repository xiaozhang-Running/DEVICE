using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceWarehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRawMaterialInboundItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RawMaterialInbound_RawMaterial_RawMaterialId",
                table: "RawMaterialInbound");

            migrationBuilder.DropIndex(
                name: "IX_RawMaterialInbound_RawMaterialId",
                table: "RawMaterialInbound");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "RawMaterialInbound");

            migrationBuilder.DropColumn(
                name: "RawMaterialId",
                table: "RawMaterialInbound");

            migrationBuilder.CreateTable(
                name: "RawMaterialInboundItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InboundId = table.Column<int>(type: "int", nullable: false),
                    RawMaterialId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawMaterialInboundItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RawMaterialInboundItem_RawMaterialInbound_InboundId",
                        column: x => x.InboundId,
                        principalTable: "RawMaterialInbound",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RawMaterialInboundItem_RawMaterial_RawMaterialId",
                        column: x => x.RawMaterialId,
                        principalTable: "RawMaterial",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RawMaterialInboundItem_InboundId",
                table: "RawMaterialInboundItem",
                column: "InboundId");

            migrationBuilder.CreateIndex(
                name: "IX_RawMaterialInboundItem_RawMaterialId",
                table: "RawMaterialInboundItem",
                column: "RawMaterialId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RawMaterialInboundItem");

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "RawMaterialInbound",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RawMaterialId",
                table: "RawMaterialInbound",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_RawMaterialInbound_RawMaterialId",
                table: "RawMaterialInbound",
                column: "RawMaterialId");

            migrationBuilder.AddForeignKey(
                name: "FK_RawMaterialInbound_RawMaterial_RawMaterialId",
                table: "RawMaterialInbound",
                column: "RawMaterialId",
                principalTable: "RawMaterial",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
