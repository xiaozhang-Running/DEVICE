using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceWarehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAccessoriesField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Accessories",
                table: "SpecialEquipment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Accessories",
                table: "GeneralEquipment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Accessories",
                table: "Consumables",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE SpecialEquipment 
                SET Accessories = Remark,
                    Remark = NULL
                WHERE Remark IS NOT NULL AND Remark != ''");

            migrationBuilder.Sql(@"
                UPDATE GeneralEquipment 
                SET Accessories = Remark,
                    Remark = NULL
                WHERE Remark IS NOT NULL AND Remark != ''");

            migrationBuilder.Sql(@"
                UPDATE Consumables 
                SET Accessories = Remark,
                    Remark = NULL
                WHERE Remark IS NOT NULL AND Remark != ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE SpecialEquipment 
                SET Remark = Accessories
                WHERE Accessories IS NOT NULL AND Accessories != ''");

            migrationBuilder.Sql(@"
                UPDATE GeneralEquipment 
                SET Remark = Accessories
                WHERE Accessories IS NOT NULL AND Accessories != ''");

            migrationBuilder.Sql(@"
                UPDATE Consumables 
                SET Remark = Accessories
                WHERE Accessories IS NOT NULL AND Accessories != ''");

            migrationBuilder.DropColumn(
                name: "Accessories",
                table: "SpecialEquipment");

            migrationBuilder.DropColumn(
                name: "Accessories",
                table: "GeneralEquipment");

            migrationBuilder.DropColumn(
                name: "Accessories",
                table: "Consumables");
        }
    }
}
