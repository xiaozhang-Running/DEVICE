using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceWarehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixProjectInboundStatusDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 修改Status列的默认值为'待入库'
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('ProjectInbound') AND name = 'DF_ProjectInbound_Status')
                BEGIN
                    ALTER TABLE ProjectInbound DROP CONSTRAINT DF_ProjectInbound_Status
                END
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ProjectInbound ADD CONSTRAINT DF_ProjectInbound_Status DEFAULT '待入库' FOR Status
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('ProjectInbound') AND name = 'DF_ProjectInbound_Status')
                BEGIN
                    ALTER TABLE ProjectInbound DROP CONSTRAINT DF_ProjectInbound_Status
                END
            ");
        }
    }
}
