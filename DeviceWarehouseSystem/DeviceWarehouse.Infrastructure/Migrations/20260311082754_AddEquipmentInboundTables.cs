using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceWarehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEquipmentInboundTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Location",
                table: "EquipmentInboundItem",
                newName: "Status");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "SpecialEquipment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "GeneralEquipment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Consumables",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "SpecialEquipment");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "GeneralEquipment");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Consumables");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "EquipmentInboundItem",
                newName: "Location");
        }
    }
}
