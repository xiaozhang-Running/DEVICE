﻿﻿﻿﻿﻿﻿﻿﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceWarehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProjectInboundStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 更新现有记录的Status字段值
            migrationBuilder.Sql("UPDATE ProjectInbound SET Status = CASE WHEN IsCompleted = 1 THEN '已完成' ELSE '部分入库' END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 不需要回滚操作
        }
    }
}
