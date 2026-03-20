using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceWarehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectInboundOutboundRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectInbound_ProjectOutbound_ProjectOutboundId",
                table: "ProjectInbound");

            migrationBuilder.DropIndex(
                name: "IX_ProjectInbound_ProjectOutboundId",
                table: "ProjectInbound");

            migrationBuilder.DropColumn(
                name: "ProjectOutboundId",
                table: "ProjectInbound");

            migrationBuilder.CreateTable(
                name: "ProjectInboundOutbound",
                columns: table => new
                {
                    ProjectInboundId = table.Column<int>(type: "int", nullable: false),
                    ProjectOutboundId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectInboundOutbound", x => new { x.ProjectInboundId, x.ProjectOutboundId });
                    table.ForeignKey(
                        name: "FK_ProjectInboundOutbound_ProjectInbound_ProjectInboundId",
                        column: x => x.ProjectInboundId,
                        principalTable: "ProjectInbound",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectInboundOutbound_ProjectOutbound_ProjectOutboundId",
                        column: x => x.ProjectOutboundId,
                        principalTable: "ProjectOutbound",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectInboundOutbound_ProjectOutboundId",
                table: "ProjectInboundOutbound",
                column: "ProjectOutboundId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectInboundOutbound");

            migrationBuilder.AddColumn<int>(
                name: "ProjectOutboundId",
                table: "ProjectInbound",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectInbound_ProjectOutboundId",
                table: "ProjectInbound",
                column: "ProjectOutboundId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectInbound_ProjectOutbound_ProjectOutboundId",
                table: "ProjectInbound",
                column: "ProjectOutboundId",
                principalTable: "ProjectOutbound",
                principalColumn: "Id");
        }
    }
}
