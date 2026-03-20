using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceWarehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectOutboundV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AssetType",
                table: "ProjectOutboundItem",
                newName: "ItemType");

            migrationBuilder.RenameColumn(
                name: "AssetId",
                table: "ProjectOutboundItem",
                newName: "ItemId");

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "ProjectOutboundItem",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ItemName",
                table: "ProjectOutboundItem",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "ProjectOutboundItem",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "ProjectOutboundItem",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProjectName",
                table: "ProjectOutbound",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TotalQuantity",
                table: "ProjectOutbound",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Brand",
                table: "ProjectOutboundItem");

            migrationBuilder.DropColumn(
                name: "ItemName",
                table: "ProjectOutboundItem");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "ProjectOutboundItem");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "ProjectOutboundItem");

            migrationBuilder.DropColumn(
                name: "ProjectName",
                table: "ProjectOutbound");

            migrationBuilder.DropColumn(
                name: "TotalQuantity",
                table: "ProjectOutbound");

            migrationBuilder.RenameColumn(
                name: "ItemType",
                table: "ProjectOutboundItem",
                newName: "AssetType");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "ProjectOutboundItem",
                newName: "AssetId");
        }
    }
}
