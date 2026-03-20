using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceWarehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EquipmentType",
                table: "EquipmentInboundItem");

            migrationBuilder.AddColumn<int>(
                name: "TemplateId",
                table: "SpecialEquipment",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TemplateId",
                table: "GeneralEquipment",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TemplateId",
                table: "Consumables",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DeviceTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceType = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Model = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Specification = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceTemplates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SpecialEquipment_TemplateId",
                table: "SpecialEquipment",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralEquipment_TemplateId",
                table: "GeneralEquipment",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Consumables_TemplateId",
                table: "Consumables",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTemplates_DeviceType",
                table: "DeviceTemplates",
                column: "DeviceType");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTemplates_Name",
                table: "DeviceTemplates",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTemplates_Name_Brand_Model",
                table: "DeviceTemplates",
                columns: new[] { "Name", "Brand", "Model" });

            migrationBuilder.AddForeignKey(
                name: "FK_Consumables_DeviceTemplates_TemplateId",
                table: "Consumables",
                column: "TemplateId",
                principalTable: "DeviceTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GeneralEquipment_DeviceTemplates_TemplateId",
                table: "GeneralEquipment",
                column: "TemplateId",
                principalTable: "DeviceTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SpecialEquipment_DeviceTemplates_TemplateId",
                table: "SpecialEquipment",
                column: "TemplateId",
                principalTable: "DeviceTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consumables_DeviceTemplates_TemplateId",
                table: "Consumables");

            migrationBuilder.DropForeignKey(
                name: "FK_GeneralEquipment_DeviceTemplates_TemplateId",
                table: "GeneralEquipment");

            migrationBuilder.DropForeignKey(
                name: "FK_SpecialEquipment_DeviceTemplates_TemplateId",
                table: "SpecialEquipment");

            migrationBuilder.DropTable(
                name: "DeviceTemplates");

            migrationBuilder.DropIndex(
                name: "IX_SpecialEquipment_TemplateId",
                table: "SpecialEquipment");

            migrationBuilder.DropIndex(
                name: "IX_GeneralEquipment_TemplateId",
                table: "GeneralEquipment");

            migrationBuilder.DropIndex(
                name: "IX_Consumables_TemplateId",
                table: "Consumables");

            migrationBuilder.DropColumn(
                name: "TemplateId",
                table: "SpecialEquipment");

            migrationBuilder.DropColumn(
                name: "TemplateId",
                table: "GeneralEquipment");

            migrationBuilder.DropColumn(
                name: "TemplateId",
                table: "Consumables");

            migrationBuilder.AddColumn<int>(
                name: "EquipmentType",
                table: "EquipmentInboundItem",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
