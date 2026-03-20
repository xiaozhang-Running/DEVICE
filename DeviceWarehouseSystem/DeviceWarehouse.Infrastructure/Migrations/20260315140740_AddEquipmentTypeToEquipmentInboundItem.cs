using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceWarehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEquipmentTypeToEquipmentInboundItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EquipmentType",
                table: "EquipmentInboundItem",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EquipmentType",
                table: "EquipmentInboundItem");
        }
    }
}
