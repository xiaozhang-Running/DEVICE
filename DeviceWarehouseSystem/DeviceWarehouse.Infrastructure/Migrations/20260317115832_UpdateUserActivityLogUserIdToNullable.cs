using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceWarehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserActivityLogUserIdToNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageContentType",
                table: "SpecialEquipment");

            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "SpecialEquipment");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "SpecialEquipment");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "ScrapEquipment");

            migrationBuilder.DropColumn(
                name: "ImageContentType",
                table: "GeneralEquipment");

            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "GeneralEquipment");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "GeneralEquipment");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "UserActivityLogs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "UserActivityLogs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageContentType",
                table: "SpecialEquipment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "SpecialEquipment",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "SpecialEquipment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "ScrapEquipment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageContentType",
                table: "GeneralEquipment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "GeneralEquipment",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "GeneralEquipment",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
