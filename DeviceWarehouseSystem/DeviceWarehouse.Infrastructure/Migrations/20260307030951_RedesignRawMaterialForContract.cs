using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceWarehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RedesignRawMaterialForContract : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropIndex(
                name: "IX_RawMaterial_MaterialName",
                table: "RawMaterial");

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
                name: "ImageUrl",
                table: "RawMaterial");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "RawMaterial");

            migrationBuilder.DropColumn(
                name: "MaterialName",
                table: "RawMaterial");

            migrationBuilder.DropColumn(
                name: "MaterialStatus",
                table: "RawMaterial");

            migrationBuilder.DropColumn(
                name: "RemainingQuantity",
                table: "RawMaterial");

            migrationBuilder.DropColumn(
                name: "RawMaterialId",
                table: "OutboundOrderItems");

            migrationBuilder.DropColumn(
                name: "RawMaterialId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "RawMaterialId",
                table: "InboundOrderItems");

            migrationBuilder.AlterColumn<string>(
                name: "Unit",
                table: "RawMaterial",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Supplier",
                table: "RawMaterial",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Specification",
                table: "RawMaterial",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Remark",
                table: "RawMaterial",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "RawMaterial",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccount",
                table: "RawMaterial",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "RawMaterial",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ContractDate",
                table: "RawMaterial",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InvoiceAmount",
                table: "RawMaterial",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PaidAmount",
                table: "RawMaterial",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentRemark",
                table: "RawMaterial",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "RawMaterial",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "RawMaterial",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RawMaterial_ProductName",
                table: "RawMaterial",
                column: "ProductName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RawMaterial_ProductName",
                table: "RawMaterial");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "RawMaterial");

            migrationBuilder.DropColumn(
                name: "BankAccount",
                table: "RawMaterial");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "RawMaterial");

            migrationBuilder.DropColumn(
                name: "ContractDate",
                table: "RawMaterial");

            migrationBuilder.DropColumn(
                name: "InvoiceAmount",
                table: "RawMaterial");

            migrationBuilder.DropColumn(
                name: "PaidAmount",
                table: "RawMaterial");

            migrationBuilder.DropColumn(
                name: "PaymentRemark",
                table: "RawMaterial");

            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "RawMaterial");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "RawMaterial");

            migrationBuilder.AlterColumn<string>(
                name: "Unit",
                table: "RawMaterial",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Supplier",
                table: "RawMaterial",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Specification",
                table: "RawMaterial",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Remark",
                table: "RawMaterial",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "RawMaterial",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "RawMaterial",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaterialName",
                table: "RawMaterial",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MaterialStatus",
                table: "RawMaterial",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RemainingQuantity",
                table: "RawMaterial",
                type: "int",
                nullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_RawMaterial_MaterialName",
                table: "RawMaterial",
                column: "MaterialName");

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
    }
}
