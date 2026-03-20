using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceWarehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateApplicationDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InboundOrderItems_SpecialEquipment_SpecialEquipmentId",
                table: "InboundOrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OutboundOrderItems_SpecialEquipment_SpecialEquipmentId",
                table: "OutboundOrderItems");

            migrationBuilder.AlterColumn<string>(
                name: "Model",
                table: "SpecialEquipment",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Brand",
                table: "SpecialEquipment",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Model",
                table: "GeneralEquipment",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeviceName",
                table: "GeneralEquipment",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Brand",
                table: "GeneralEquipment",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Consumables",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ModelSpecification",
                table: "Consumables",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Brand",
                table: "Consumables",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpecialEquipment_DeviceName_Brand_Model",
                table: "SpecialEquipment",
                columns: new[] { "DeviceName", "Brand", "Model" });

            migrationBuilder.CreateIndex(
                name: "IX_SpecialEquipment_Quantity",
                table: "SpecialEquipment",
                column: "Quantity");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralEquipment_DeviceName",
                table: "GeneralEquipment",
                column: "DeviceName");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralEquipment_DeviceName_Brand_Model",
                table: "GeneralEquipment",
                columns: new[] { "DeviceName", "Brand", "Model" });

            migrationBuilder.CreateIndex(
                name: "IX_GeneralEquipment_Quantity",
                table: "GeneralEquipment",
                column: "Quantity");

            migrationBuilder.CreateIndex(
                name: "IX_Consumables_Name_Brand_ModelSpecification",
                table: "Consumables",
                columns: new[] { "Name", "Brand", "ModelSpecification" });

            migrationBuilder.CreateIndex(
                name: "IX_Consumables_RemainingQuantity",
                table: "Consumables",
                column: "RemainingQuantity");

            migrationBuilder.AddForeignKey(
                name: "FK_InboundOrderItems_SpecialEquipment_SpecialEquipmentId",
                table: "InboundOrderItems",
                column: "SpecialEquipmentId",
                principalTable: "SpecialEquipment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OutboundOrderItems_SpecialEquipment_SpecialEquipmentId",
                table: "OutboundOrderItems",
                column: "SpecialEquipmentId",
                principalTable: "SpecialEquipment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InboundOrderItems_SpecialEquipment_SpecialEquipmentId",
                table: "InboundOrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OutboundOrderItems_SpecialEquipment_SpecialEquipmentId",
                table: "OutboundOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_SpecialEquipment_DeviceName_Brand_Model",
                table: "SpecialEquipment");

            migrationBuilder.DropIndex(
                name: "IX_SpecialEquipment_Quantity",
                table: "SpecialEquipment");

            migrationBuilder.DropIndex(
                name: "IX_GeneralEquipment_DeviceName",
                table: "GeneralEquipment");

            migrationBuilder.DropIndex(
                name: "IX_GeneralEquipment_DeviceName_Brand_Model",
                table: "GeneralEquipment");

            migrationBuilder.DropIndex(
                name: "IX_GeneralEquipment_Quantity",
                table: "GeneralEquipment");

            migrationBuilder.DropIndex(
                name: "IX_Consumables_Name_Brand_ModelSpecification",
                table: "Consumables");

            migrationBuilder.DropIndex(
                name: "IX_Consumables_RemainingQuantity",
                table: "Consumables");

            migrationBuilder.AlterColumn<string>(
                name: "Model",
                table: "SpecialEquipment",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Brand",
                table: "SpecialEquipment",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Model",
                table: "GeneralEquipment",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeviceName",
                table: "GeneralEquipment",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Brand",
                table: "GeneralEquipment",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Consumables",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ModelSpecification",
                table: "Consumables",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Brand",
                table: "Consumables",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_InboundOrderItems_SpecialEquipment_SpecialEquipmentId",
                table: "InboundOrderItems",
                column: "SpecialEquipmentId",
                principalTable: "SpecialEquipment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OutboundOrderItems_SpecialEquipment_SpecialEquipmentId",
                table: "OutboundOrderItems",
                column: "SpecialEquipmentId",
                principalTable: "SpecialEquipment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
