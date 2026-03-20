using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceWarehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNameSequenceColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 只添加缺失的NameSequence字段
            migrationBuilder.AddColumn<int>(
                name: "NameSequence",
                table: "SpecialEquipment",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NameSequence",
                table: "GeneralEquipment",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 只删除添加的NameSequence字段
            migrationBuilder.DropColumn(
                name: "NameSequence",
                table: "SpecialEquipment");

            migrationBuilder.DropColumn(
                name: "NameSequence",
                table: "GeneralEquipment");
        }
    }
}
