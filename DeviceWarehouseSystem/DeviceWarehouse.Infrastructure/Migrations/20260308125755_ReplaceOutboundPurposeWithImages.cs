using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceWarehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceOutboundPurposeWithImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OutboundPurpose",
                table: "ProjectOutbound");

            migrationBuilder.AddColumn<string>(
                name: "PurposeImages",
                table: "ProjectOutbound",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PurposeImages",
                table: "ProjectOutbound");

            migrationBuilder.AddColumn<string>(
                name: "OutboundPurpose",
                table: "ProjectOutbound",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
