using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceWarehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUsedAndRemainingQuantityToRawMaterial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "RawMaterial",
                newName: "UsedQuantity");

            migrationBuilder.AddColumn<int>(
                name: "RemainingQuantity",
                table: "RawMaterial",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalQuantity",
                table: "RawMaterial",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RemainingQuantity",
                table: "RawMaterial");

            migrationBuilder.DropColumn(
                name: "TotalQuantity",
                table: "RawMaterial");

            migrationBuilder.RenameColumn(
                name: "UsedQuantity",
                table: "RawMaterial",
                newName: "Quantity");
        }
    }
}
