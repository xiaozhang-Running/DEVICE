
using AutoMapper;
using DeviceWarehouse.Application.Mappings;
using DeviceWarehouse.Application.Services;
using DeviceWarehouse.Application.Interfaces;
using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Domain.Interfaces;
using DeviceWarehouse.Infrastructure.Data;
using DeviceWarehouse.Infrastructure.Repositories;
using DeviceWarehouse.Infrastructure.Services;
using DeviceWarehouse.API.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("DeviceWarehouse.Infrastructure")));

builder.Services.AddScoped<ISpecialEquipmentRepository, SpecialEquipmentRepository>();
builder.Services.AddScoped<IGeneralEquipmentRepository, GeneralEquipmentRepository>();
builder.Services.AddScoped<IConsumableRepository, ConsumableRepository>();
builder.Services.AddScoped<IRawMaterialRepository, RawMaterialRepository>();
builder.Services.AddScoped<IRawMaterialInboundRepository, RawMaterialInboundRepository>();
builder.Services.AddScoped<IRawMaterialOutboundRepository, RawMaterialOutboundRepository>();
builder.Services.AddScoped<IEquipmentInboundRepository, EquipmentInboundRepository>();
builder.Services.AddScoped<IProjectOutboundRepository, ProjectOutboundRepository>();
builder.Services.AddScoped<IProjectInboundRepository, ProjectInboundRepository>();
builder.Services.AddScoped<IInboundOrderRepository, InboundOrderRepository>();
builder.Services.AddScoped<IOutboundOrderRepository, OutboundOrderRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IInventoryTransactionRepository, InventoryTransactionRepository>();
builder.Services.AddScoped<IDeviceTemplateRepository, DeviceTemplateRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IUserActivityLogRepository, UserActivityLogRepository>();
builder.Services.AddScoped<IScrapEquipmentRepository, ScrapEquipmentRepository>();

builder.Services.AddScoped<ISpecialEquipmentService, SpecialEquipmentService>();
builder.Services.AddScoped<IGeneralEquipmentService, GeneralEquipmentService>();
builder.Services.AddScoped<IConsumableService, ConsumableService>();
builder.Services.AddScoped<IRawMaterialService, RawMaterialService>();
builder.Services.AddScoped<IRawMaterialInboundService, RawMaterialInboundService>();
builder.Services.AddScoped<IRawMaterialOutboundService, RawMaterialOutboundService>();
builder.Services.AddScoped<IEquipmentInboundService, EquipmentInboundService>();
builder.Services.AddScoped<IProjectOutboundService, ProjectOutboundService>();
builder.Services.AddScoped<IProjectInboundService, ProjectInboundService>();
builder.Services.AddScoped<IDeviceTemplateService, DeviceTemplateService>();
// builder.Services.AddScoped<IInboundOrderService, InboundOrderService>();
builder.Services.AddScoped<IOutboundOrderService, OutboundOrderService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
// builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IUserActivityLogService, UserActivityLogService>();
builder.Services.AddScoped<IScrapEquipmentService, ScrapEquipmentService>();

// 使用更具体的方式配置 AutoMapper，解决重载歧义问题
var mapperConfig = new MapperConfiguration(cfg =>
{
    cfg.AddProfile<MappingProfile>();
});
builder.Services.AddSingleton(mapperConfig.CreateMapper());

// 添加内存缓存
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ICacheService, MemoryCacheService>();

// 添加响应压缩
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "仓库管理系统 API", 
        Version = "v1" 
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 配置JWT认证
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your-secret-key-here-at-least-32-characters"))
        };
    });

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase, true));
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseResponseCompression();
app.UseCors("AllowAll");
// 暂时禁用HTTPS重定向，避免SSL证书错误
// app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// 添加用户活动日志中间件
app.UseUserActivityLogging();

// 添加静态文件服务
app.UseStaticFiles();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
    // 取消注释SeedData.Seed方法的调用，初始化数据库数据
    DeviceWarehouse.Infrastructure.Data.SeedData.Seed(context);
}

app.Run();
