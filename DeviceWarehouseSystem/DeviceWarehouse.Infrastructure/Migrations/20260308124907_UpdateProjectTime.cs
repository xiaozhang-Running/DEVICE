using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceWarehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProjectTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Department",
                table: "ProjectOutbound");

            migrationBuilder.AddColumn<string>(
                name: "ProjectTime",
                table: "ProjectOutbound",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProjectTime",
                table: "ProjectOutbound");

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "ProjectOutbound",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
