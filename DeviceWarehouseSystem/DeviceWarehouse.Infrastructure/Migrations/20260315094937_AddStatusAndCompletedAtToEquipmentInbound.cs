using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceWarehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusAndCompletedAtToEquipmentInbound : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "EquipmentInbound",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "EquipmentInbound",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "EquipmentInbound");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "EquipmentInbound");

            migrationBuilder.AddColumn<string>(
                name: "Supplier",
                table: "EquipmentInbound",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
