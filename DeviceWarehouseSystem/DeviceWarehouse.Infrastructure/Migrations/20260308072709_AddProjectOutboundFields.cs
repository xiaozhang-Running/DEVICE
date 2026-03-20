using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceWarehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectOutboundFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "ProjectOutbound",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OutboundPurpose",
                table: "ProjectOutbound",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProjectCode",
                table: "ProjectOutbound",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProjectManager",
                table: "ProjectOutbound",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Department",
                table: "ProjectOutbound");

            migrationBuilder.DropColumn(
                name: "OutboundPurpose",
                table: "ProjectOutbound");

            migrationBuilder.DropColumn(
                name: "ProjectCode",
                table: "ProjectOutbound");

            migrationBuilder.DropColumn(
                name: "ProjectManager",
                table: "ProjectOutbound");
        }
    }
}
