using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceWarehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameSupplierToDeliveryPersonForAll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Supplier",
                table: "RawMaterialInbound",
                newName: "DeliveryPerson");

            migrationBuilder.RenameColumn(
                name: "Supplier",
                table: "InboundOrders",
                newName: "DeliveryPerson");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeliveryPerson",
                table: "RawMaterialInbound",
                newName: "Supplier");

            migrationBuilder.RenameColumn(
                name: "DeliveryPerson",
                table: "InboundOrders",
                newName: "Supplier");
        }
    }
}
