using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceWarehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Company",
                table: "ProjectOutbound");

            migrationBuilder.DropColumn(
                name: "ContactPerson",
                table: "ProjectOutbound");

            migrationBuilder.DropColumn(
                name: "Operator",
                table: "ProjectOutbound");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Company",
                table: "ProjectOutbound",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPerson",
                table: "ProjectOutbound",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Operator",
                table: "ProjectOutbound",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
