using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceWarehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddScrapEquipment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FaultReason",
                table: "SpecialEquipment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RepairDate",
                table: "SpecialEquipment",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepairPerson",
                table: "SpecialEquipment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RepairStatus",
                table: "SpecialEquipment",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FaultReason",
                table: "GeneralEquipment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RepairDate",
                table: "GeneralEquipment",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepairPerson",
                table: "GeneralEquipment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RepairStatus",
                table: "GeneralEquipment",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ScrapEquipment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SpecialEquipmentId = table.Column<int>(type: "int", nullable: true),
                    GeneralEquipmentId = table.Column<int>(type: "int", nullable: true),
                    DeviceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeviceCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Specification = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeviceType = table.Column<int>(type: "int", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Company = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Accessories = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScrapReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScrapDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScrappedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScrapEquipment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScrapEquipment_GeneralEquipment_GeneralEquipmentId",
                        column: x => x.GeneralEquipmentId,
                        principalTable: "GeneralEquipment",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ScrapEquipment_SpecialEquipment_SpecialEquipmentId",
                        column: x => x.SpecialEquipmentId,
                        principalTable: "SpecialEquipment",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScrapEquipment_DeviceCode",
                table: "ScrapEquipment",
                column: "DeviceCode");

            migrationBuilder.CreateIndex(
                name: "IX_ScrapEquipment_DeviceType",
                table: "ScrapEquipment",
                column: "DeviceType");

            migrationBuilder.CreateIndex(
                name: "IX_ScrapEquipment_GeneralEquipmentId",
                table: "ScrapEquipment",
                column: "GeneralEquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ScrapEquipment_ScrapDate",
                table: "ScrapEquipment",
                column: "ScrapDate");

            migrationBuilder.CreateIndex(
                name: "IX_ScrapEquipment_SpecialEquipmentId",
                table: "ScrapEquipment",
                column: "SpecialEquipmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScrapEquipment");

            migrationBuilder.DropColumn(
                name: "FaultReason",
                table: "SpecialEquipment");

            migrationBuilder.DropColumn(
                name: "RepairDate",
                table: "SpecialEquipment");

            migrationBuilder.DropColumn(
                name: "RepairPerson",
                table: "SpecialEquipment");

            migrationBuilder.DropColumn(
                name: "RepairStatus",
                table: "SpecialEquipment");

            migrationBuilder.DropColumn(
                name: "FaultReason",
                table: "GeneralEquipment");

            migrationBuilder.DropColumn(
                name: "RepairDate",
                table: "GeneralEquipment");

            migrationBuilder.DropColumn(
                name: "RepairPerson",
                table: "GeneralEquipment");

            migrationBuilder.DropColumn(
                name: "RepairStatus",
                table: "GeneralEquipment");
        }
    }
}
