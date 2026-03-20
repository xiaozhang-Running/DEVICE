using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceWarehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboundFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactPerson",
                table: "ProjectOutbound",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "ProjectOutbound",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Handler",
                table: "ProjectOutbound",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OutboundType",
                table: "ProjectOutbound",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnDate",
                table: "ProjectOutbound",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsageLocation",
                table: "ProjectOutbound",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WarehouseKeeper",
                table: "ProjectOutbound",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactPerson",
                table: "ProjectOutbound");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                table: "ProjectOutbound");

            migrationBuilder.DropColumn(
                name: "Handler",
                table: "ProjectOutbound");

            migrationBuilder.DropColumn(
                name: "OutboundType",
                table: "ProjectOutbound");

            migrationBuilder.DropColumn(
                name: "ReturnDate",
                table: "ProjectOutbound");

            migrationBuilder.DropColumn(
                name: "UsageLocation",
                table: "ProjectOutbound");

            migrationBuilder.DropColumn(
                name: "WarehouseKeeper",
                table: "ProjectOutbound");
        }
    }
}
