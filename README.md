# 仓库管理系统 (DeviceWarehouseSystem)

## 项目介绍

仓库管理系统是一个基于.NET 10.0开发的企业级仓库管理解决方案，旨在帮助企业高效管理设备、耗材和原材料的入库、出库、库存跟踪等业务流程。

## 系统架构

系统采用分层架构设计，遵循领域驱动设计(DDD)原则：

- **API层**：提供RESTful接口，处理HTTP请求和响应
- **Application层**：实现业务逻辑，协调各领域服务
- **Domain层**：定义核心业务实体和领域服务
- **Infrastructure层**：提供数据访问、缓存、外部服务等基础设施

## 技术栈

- **后端**：.NET 10.0, C#
- **数据库**：SQL Server
- **ORM**：Entity Framework Core
- **认证**：JWT (JSON Web Token)
- **API文档**：Swagger
- **缓存**：内存缓存
- **映射**：AutoMapper
- **跨域**：CORS

## 核心功能模块

### 1. 设备管理
- **专用设备管理**：添加、编辑、删除、查询专用设备
- **通用设备管理**：添加、编辑、删除、查询通用设备
- **设备状态跟踪**：监控设备使用状态（正常、损坏、报废）
- **设备库存管理**：实时跟踪设备库存数量

### 2. 耗材管理
- **耗材入库**：记录耗材采购和入库信息
- **耗材出库**：跟踪耗材使用和消耗
- **耗材库存监控**：实时显示耗材剩余数量

### 3. 原材料管理
- **原材料入库**：记录原材料采购和入库
- **原材料出库**：跟踪原材料使用和消耗
- **原材料库存管理**：实时监控原材料库存水平

### 4. 项目管理
- **项目出库**：为特定项目分配设备和耗材
- **项目入库**：项目结束后设备和耗材的归还
- **项目状态跟踪**：监控项目的设备使用情况

### 5. 库存管理
- **库存盘点**：定期库存盘点和调整
- **库存预警**：低库存自动预警
- **库存报表**：生成库存状态和变动报表

### 6. 用户与权限管理
- **用户管理**：添加、编辑、删除用户
- **角色管理**：定义和管理用户角色
- **权限控制**：基于角色的权限控制
- **用户活动日志**：记录用户操作日志

## 项目结构

```
DeviceWarehouseSystem/
├── DeviceWarehouse.API/           # API层
│   ├── Controllers/               # 控制器
│   ├── Middleware/                # 中间件
│   ├── Program.cs                 # 应用入口
│   └── appsettings.json           # 配置文件
├── DeviceWarehouse.Application/   # 应用层
│   ├── DTOs/                      # 数据传输对象
│   ├── Interfaces/                # 服务接口
│   ├── Mappings/                  # 对象映射
│   └── Services/                  # 业务服务
├── DeviceWarehouse.Domain/        # 领域层
│   ├── Entities/                  # 领域实体
│   ├── Enums/                     # 枚举类型
│   └── Interfaces/                # 领域服务接口
├── DeviceWarehouse.Infrastructure/ # 基础设施层
│   ├── Data/                      # 数据访问
│   ├── Repositories/              # 仓储实现
│   └── Services/                  # 基础设施服务
├── AddConsumables/                # 耗材添加工具
├── ConsumableChecker/             # 耗材检查工具
└── ConsumableImporter/            # 耗材导入工具
```

## 快速开始

### 环境要求

- .NET 10.0 SDK
- SQL Server 2019+
- Visual Studio 2022+

### 安装步骤

1. **克隆项目**

   ```bash
   git clone <repository-url>
   cd DeviceWarehouseSystem
   ```

2. **配置数据库连接**

   修改 `DeviceWarehouse.API/appsettings.json` 文件中的数据库连接字符串：

   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=<server-name>;Database=DeviceWarehouse;Trusted_Connection=True;MultipleActiveResultSets=true"
   }
   ```

3. **初始化数据库**

   项目启动时会自动创建数据库并初始化基础数据：

   ```csharp
   // 在 Program.cs 中
   using (var scope = app.Services.CreateScope())
   {
       var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
       context.Database.EnsureCreated();
       DeviceWarehouse.Infrastructure.Data.SeedData.Seed(context);
   }
   ```

4. **运行项目**

   - 在 Visual Studio 中：按 F5 启动调试
   - 或使用命令行：

     ```bash
     cd DeviceWarehouse.API
     dotnet run
     ```

5. **访问API文档**

   项目启动后，可通过以下地址访问 Swagger API 文档：
   `http://localhost:<port>/swagger`

## API接口

### 核心接口

- **设备管理**
  - `GET /api/SpecialEquipments` - 获取专用设备列表
  - `POST /api/SpecialEquipments` - 添加专用设备
  - `GET /api/GeneralEquipments` - 获取通用设备列表
  - `POST /api/GeneralEquipments` - 添加通用设备

- **耗材管理**
  - `GET /api/Consumables` - 获取耗材列表
  - `POST /api/Consumables` - 添加耗材

- **项目管理**
  - `GET /api/ProjectOutbounds` - 获取项目出库单列表
  - `POST /api/ProjectOutbounds` - 创建项目出库单
  - `PUT /api/ProjectOutbounds/{id}/complete` - 完成项目出库
  - `GET /api/ProjectInbounds` - 获取项目入库单列表
  - `POST /api/ProjectInbounds` - 创建项目入库单

- **库存管理**
  - `GET /api/Inventory` - 获取库存状态
  - `GET /api/Inventory/transactions` - 获取库存交易记录

- **用户管理**
  - `GET /api/Users` - 获取用户列表
  - `POST /api/Users` - 添加用户
  - `POST /api/Users/login` - 用户登录

## 系统特点

1. **模块化设计**：采用分层架构，各模块职责清晰，易于维护和扩展
2. **高性能**：使用内存缓存、响应压缩等技术提升系统性能
3. **安全性**：实现JWT认证，保护API安全
4. **可扩展性**：预留了插件和扩展点，支持功能扩展
5. **用户友好**：提供Swagger API文档，方便前端开发和测试
6. **实时监控**：记录用户活动日志，便于系统监控和审计

## 示例代码

### 创建项目出库单

```csharp
// 前端调用示例
const response = await fetch('/api/ProjectOutbounds', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': 'Bearer ' + token
  },
  body: JSON.stringify({
    outboundNumber: 'XMCK202503080001',
    outboundDate: '2025-03-08',
    projectName: '测试项目',
    projectCode: 'PROJ-001',
    projectManager: '张三',
    recipient: '李四',
    outboundType: '项目使用',
    projectTime: '2025-03-08 至 2025-03-15',
    contactPhone: '13800138000',
    usageLocation: '实验室',
    returnDate: '2025-03-16',
    handler: '王五',
    warehouseKeeper: '赵六',
    remark: '测试出库',
    items: [
      {
        itemType: 1, // 专用设备
        itemId: 1,
        itemName: '光谱分析仪',
        deviceCode: 'DEV-001',
        brand: 'Agilent',
        model: '7890A',
        quantity: 1,
        unit: '台',
        accessories: '电源线、说明书',
        remark: '精密设备',
        deviceStatus: '正常'
      }
    ]
  })
});

const result = await response.json();
console.log('出库单创建成功:', result);
```

## 故障排除

### 常见问题

1. **数据库连接失败**
   - 检查连接字符串是否正确
   - 确保SQL Server服务正在运行
   - 验证数据库权限

2. **API调用返回401错误**
   - 检查JWT令牌是否有效
   - 验证用户权限

3. **库存更新失败**
   - 检查库存是否充足
   - 验证设备状态是否正确

4. **性能问题**
   - 检查数据库索引
   - 考虑增加缓存时间
   - 优化查询语句

## 贡献

欢迎贡献代码和提出建议！请遵循以下步骤：

1. Fork 项目
2. 创建功能分支 (`git checkout -b feature/amazing-feature`)
3. 提交更改 (`git commit -m 'Add some amazing feature'`)
4. 推送到分支 (`git push origin feature/amazing-feature`)
5. 打开 Pull Request

## 许可证

本项目采用 MIT 许可证 - 详见 [LICENSE](LICENSE) 文件

## 联系方式

- 项目维护者：[Your Name]
- 邮箱：[your.email@example.com]
- 项目地址：[https://github.com/yourusername/DeviceWarehouseSystem](https://github.com/yourusername/DeviceWarehouseSystem)

---

**注意**：本系统仅供企业内部使用，请勿用于生产环境而不进行适当的安全评估和配置。