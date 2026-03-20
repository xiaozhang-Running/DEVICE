using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceWarehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DeviceName",
                table: "SpecialEquipment",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialEquipment_DeviceName",
                table: "SpecialEquipment",
                column: "DeviceName");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialEquipment_DeviceType",
                table: "SpecialEquipment",
                column: "DeviceType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SpecialEquipment_DeviceName",
                table: "SpecialEquipment");

            migrationBuilder.DropIndex(
                name: "IX_SpecialEquipment_DeviceType",
                table: "SpecialEquipment");

            migrationBuilder.AlterColumn<string>(
                name: "DeviceName",
                table: "SpecialEquipment",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
