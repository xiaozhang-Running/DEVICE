using DeviceWarehouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeviceWarehouse.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options) { }

    public DbSet<SpecialEquipment> SpecialEquipments { get; set; } = null!;
    public DbSet<GeneralEquipment> GeneralEquipments { get; set; } = null!;
    public DbSet<Consumable> Consumables { get; set; } = null!;
    public DbSet<RawMaterial> RawMaterials { get; set; } = null!;
    public DbSet<RawMaterialInbound> RawMaterialInbounds { get; set; } = null!;
    public DbSet<RawMaterialInboundItem> RawMaterialInboundItems { get; set; } = null!;
    public DbSet<RawMaterialOutbound> RawMaterialOutbounds { get; set; } = null!;
    public DbSet<RawMaterialOutboundItem> RawMaterialOutboundItems { get; set; } = null!;
    public DbSet<EquipmentInbound> EquipmentInbounds { get; set; } = null!;
    public DbSet<EquipmentInboundItem> EquipmentInboundItems { get; set; } = null!;
    public DbSet<ProjectOutbound> ProjectOutbounds { get; set; } = null!;
    public DbSet<ProjectOutboundItem> ProjectOutboundItems { get; set; } = null!;
    public DbSet<ProjectInbound> ProjectInbounds { get; set; } = null!;
    public DbSet<ProjectInboundItem> ProjectInboundItems { get; set; } = null!;
    public DbSet<InboundOrder> InboundOrders { get; set; } = null!;
    public DbSet<InboundOrderItem> InboundOrderItems { get; set; } = null!;
    public DbSet<OutboundOrder> OutboundOrders { get; set; } = null!;
    public DbSet<OutboundOrderItem> OutboundOrderItems { get; set; } = null!;
    public DbSet<Inventory> Inventories { get; set; } = null!;
    public DbSet<InventoryTransaction> InventoryTransactions { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<Permission> Permissions { get; set; } = null!;
    public DbSet<UserActivityLog> UserActivityLogs { get; set; } = null!;
    public DbSet<ScrapEquipment> ScrapEquipments { get; set; } = null!;
    public DbSet<Image> Images { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<SpecialEquipment>()
            .ToTable("SpecialEquipment");
        
        modelBuilder.Entity<SpecialEquipment>()
            .HasIndex(d => d.DeviceCode)
            .IsUnique();
        
        modelBuilder.Entity<SpecialEquipment>()
            .HasIndex(d => d.DeviceName);
        
        modelBuilder.Entity<SpecialEquipment>()
            .HasIndex(d => d.DeviceType);
        
        modelBuilder.Entity<SpecialEquipment>()
            .HasIndex(d => d.Quantity);
        
        modelBuilder.Entity<SpecialEquipment>()
            .HasIndex(d => new { d.DeviceName, d.Brand, d.Model });
        
        modelBuilder.Entity<SpecialEquipment>()
            .HasIndex(d => d.DeviceStatus);
        
        modelBuilder.Entity<SpecialEquipment>()
            .HasIndex(d => d.UseStatus);
        
        modelBuilder.Entity<SpecialEquipment>()
            .HasIndex(d => d.Location);
        
        modelBuilder.Entity<SpecialEquipment>()
            .Property(d => d.DeviceType)
            .HasConversion<int>();
        
        modelBuilder.Entity<SpecialEquipment>()
            .Property(d => d.DeviceStatus)
            .HasConversion<int>();
        
        modelBuilder.Entity<SpecialEquipment>()
            .Property(d => d.UseStatus)
            .HasConversion<int>();
        
        modelBuilder.Entity<GeneralEquipment>()
            .ToTable("GeneralEquipment");
        
        modelBuilder.Entity<GeneralEquipment>()
            .HasIndex(d => d.DeviceCode)
            .IsUnique();
        
        modelBuilder.Entity<GeneralEquipment>()
            .HasIndex(d => d.DeviceName);
        
        modelBuilder.Entity<GeneralEquipment>()
            .HasIndex(d => d.Quantity);
        
        modelBuilder.Entity<GeneralEquipment>()
            .HasIndex(d => new { d.DeviceName, d.Brand, d.Model });
        
        modelBuilder.Entity<GeneralEquipment>()
            .HasIndex(d => d.DeviceStatus);
        
        modelBuilder.Entity<GeneralEquipment>()
            .HasIndex(d => d.UseStatus);
        
        modelBuilder.Entity<GeneralEquipment>()
            .HasIndex(d => d.Location);
        
        modelBuilder.Entity<GeneralEquipment>()
            .Property(d => d.DeviceType)
            .HasConversion<int>();
        
        modelBuilder.Entity<GeneralEquipment>()
            .Property(d => d.DeviceStatus)
            .HasConversion<int>();
        
        modelBuilder.Entity<GeneralEquipment>()
            .Property(d => d.UseStatus)
            .HasConversion<int>();
        
        modelBuilder.Entity<GeneralEquipment>()
            .HasIndex(d => d.DeviceType);

        modelBuilder.Entity<Consumable>()
            .ToTable("Consumables");

        modelBuilder.Entity<Consumable>()
            .Property(c => c.TotalQuantity)
            .HasDefaultValue(0);

        modelBuilder.Entity<Consumable>()
            .Property(c => c.OriginalQuantity)
            .HasDefaultValue(0);

        modelBuilder.Entity<Consumable>()
            .Property(c => c.UsedQuantity)
            .HasDefaultValue(0);

        modelBuilder.Entity<Consumable>()
            .Property(c => c.RemainingQuantity)
            .HasDefaultValue(0);

        modelBuilder.Entity<Consumable>()
            .HasIndex(c => c.RemainingQuantity);
        
        modelBuilder.Entity<Consumable>()
            .HasIndex(c => new { c.Name, c.Brand, c.ModelSpecification });
        
        modelBuilder.Entity<Consumable>()
            .HasIndex(c => c.TotalQuantity);
        
        modelBuilder.Entity<Consumable>()
            .HasIndex(c => c.UsedQuantity);
        
        modelBuilder.Entity<Consumable>()
            .HasIndex(c => c.Location);
        
        modelBuilder.Entity<Consumable>()
            .HasIndex(c => c.Company);
        
        modelBuilder.Entity<Consumable>()
            .HasIndex(c => c.Unit);

        modelBuilder.Entity<RawMaterial>()
            .ToTable("RawMaterial");
        
        modelBuilder.Entity<RawMaterial>()
            .HasIndex(r => r.ProductName);
        
        modelBuilder.Entity<RawMaterial>()
            .HasIndex(r => r.Supplier);
        
        modelBuilder.Entity<RawMaterial>()
            .HasIndex(r => r.RemainingQuantity);
        
        modelBuilder.Entity<InboundOrder>()
            .HasIndex(o => o.OrderCode)
            .IsUnique();
        
        modelBuilder.Entity<InboundOrder>()
            .Property(o => o.InboundType)
            .HasConversion<int>();
        
        modelBuilder.Entity<InboundOrder>()
            .Property(o => o.Status)
            .HasConversion<int>();
        
        modelBuilder.Entity<InboundOrder>()
            .Property(o => o.TotalAmount)
            .HasPrecision(18, 2);
        
        modelBuilder.Entity<OutboundOrder>()
            .HasIndex(o => o.OrderCode)
            .IsUnique();
        
        modelBuilder.Entity<OutboundOrder>()
            .Property(o => o.OutboundType)
            .HasConversion<int>();
        
        modelBuilder.Entity<OutboundOrder>()
            .Property(o => o.Status)
            .HasConversion<int>();
        
        modelBuilder.Entity<InboundOrderItem>()
            .Property(i => i.UnitPrice)
            .HasPrecision(18, 2);
        
        modelBuilder.Entity<InboundOrderItem>()
            .HasOne(i => i.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<OutboundOrderItem>()
            .HasOne(i => i.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Inventory>()
            .HasOne(i => i.SpecialEquipment)
            .WithOne(d => d.Inventory)
            .HasForeignKey<Inventory>(i => i.SpecialEquipmentId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Inventory>()
            .HasOne(i => i.GeneralEquipment)
            .WithOne(d => d.Inventory)
            .HasForeignKey<Inventory>(i => i.GeneralEquipmentId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Inventory>()
            .HasIndex(i => i.SpecialEquipmentId);
        
        modelBuilder.Entity<Inventory>()
            .HasIndex(i => i.GeneralEquipmentId);
        
        // 确保 Inventory 表中的 CurrentQuantity 与设备表中的 Quantity 保持一致
        modelBuilder.Entity<Inventory>()
            .Property(i => i.CurrentQuantity)
            .HasDefaultValue(0);
        
        modelBuilder.Entity<RawMaterialInbound>()
            .ToTable("RawMaterialInbound");
        
        modelBuilder.Entity<RawMaterialInbound>()
            .HasIndex(i => i.InboundNumber)
            .IsUnique();
        
        modelBuilder.Entity<RawMaterialInbound>()
            .HasMany(i => i.Items)
            .WithOne(item => item.Inbound)
            .HasForeignKey(item => item.InboundId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<RawMaterialInboundItem>()
            .ToTable("RawMaterialInboundItem");
        
        modelBuilder.Entity<RawMaterialInboundItem>()
            .HasOne(item => item.RawMaterial)
            .WithMany()
            .HasForeignKey(item => item.RawMaterialId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<RawMaterialOutbound>()
            .ToTable("RawMaterialOutbound");
        
        modelBuilder.Entity<RawMaterialOutbound>()
            .HasIndex(i => i.OutboundNumber)
            .IsUnique();
        
        modelBuilder.Entity<RawMaterialOutbound>()
            .HasMany(i => i.Items)
            .WithOne(item => item.Outbound)
            .HasForeignKey(item => item.OutboundId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<RawMaterialOutboundItem>()
            .ToTable("RawMaterialOutboundItem");
        
        modelBuilder.Entity<RawMaterialOutboundItem>()
            .HasOne(item => item.RawMaterial)
            .WithMany()
            .HasForeignKey(item => item.RawMaterialId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<EquipmentInbound>()
            .ToTable("EquipmentInbound");
        
        modelBuilder.Entity<EquipmentInbound>()
            .HasIndex(i => i.InboundNumber)
            .IsUnique();
        
        modelBuilder.Entity<EquipmentInbound>()
            .Property(i => i.EquipmentType)
            .HasConversion<int>();
        
        modelBuilder.Entity<EquipmentInbound>()
            .HasMany(i => i.Items)
            .WithOne(item => item.Inbound)
            .HasForeignKey(item => item.InboundId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<EquipmentInboundItem>()
            .ToTable("EquipmentInboundItem");
        
        modelBuilder.Entity<ProjectOutbound>()
            .ToTable("ProjectOutbound");
        
        modelBuilder.Entity<ProjectOutbound>()
            .HasIndex(i => i.OutboundNumber)
            .IsUnique();
        
        modelBuilder.Entity<ProjectOutbound>()
            .HasMany(i => i.Items)
            .WithOne(item => item.Outbound)
            .HasForeignKey(item => item.OutboundId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<ProjectOutboundItem>()
            .ToTable("ProjectOutboundItem");
        
        modelBuilder.Entity<ProjectInbound>()
            .ToTable("ProjectInbound");
        
        modelBuilder.Entity<ProjectInbound>()
            .Property(i => i.Status)
            .HasDefaultValue("待入库");
        
        modelBuilder.Entity<ProjectInbound>()
            .HasIndex(i => i.InboundNumber)
            .IsUnique();
        
        modelBuilder.Entity<ProjectInbound>()
            .HasMany(i => i.Items)
            .WithOne(item => item.Inbound)
            .HasForeignKey(item => item.InboundId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<ProjectInboundItem>()
            .ToTable("ProjectInboundItem");

        // 配置 ProjectInboundOutbound 多对多关系
        modelBuilder.Entity<ProjectInboundOutbound>()
            .ToTable("ProjectInboundOutbound");

        modelBuilder.Entity<ProjectInboundOutbound>()
            .HasKey(pi => new { pi.ProjectInboundId, pi.ProjectOutboundId });

        modelBuilder.Entity<ProjectInboundOutbound>()
            .HasOne(pi => pi.ProjectInbound)
            .WithMany(p => p.ProjectInboundOutbounds)
            .HasForeignKey(pi => pi.ProjectInboundId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProjectInboundOutbound>()
            .HasOne(pi => pi.ProjectOutbound)
            .WithMany(p => p.ProjectInboundOutbounds)
            .HasForeignKey(pi => pi.ProjectOutboundId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ScrapEquipment>()
            .ToTable("ScrapEquipment");
        
        modelBuilder.Entity<ScrapEquipment>()
            .HasIndex(s => s.DeviceCode);
        
        modelBuilder.Entity<ScrapEquipment>()
            .HasIndex(s => s.DeviceType);
        
        modelBuilder.Entity<ScrapEquipment>()
            .HasIndex(s => s.ScrapDate);
        
        modelBuilder.Entity<InventoryTransaction>()
            .ToTable("InventoryTransaction");
        
        modelBuilder.Entity<InventoryTransaction>()
            .HasOne(t => t.Inventory)
            .WithMany()
            .HasForeignKey(t => t.InventoryId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<InventoryTransaction>()
            .HasIndex(t => t.InventoryId);
        
        modelBuilder.Entity<InventoryTransaction>()
            .HasIndex(t => t.TransactionType);
        
        modelBuilder.Entity<InventoryTransaction>()
            .HasIndex(t => t.TransactionDate);
        
        // 配置Role和Permission之间的多对多关系
        modelBuilder.Entity<Role>()
            .HasMany(r => r.Permissions)
            .WithMany(p => p.Roles)
            .UsingEntity(j => j.ToTable("RolePermissions"));
        
        // 配置UserActivityLog和User之间的关系
        modelBuilder.Entity<UserActivityLog>()
            .HasOne(l => l.User)
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // 为User添加索引
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();
        
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
        
        // 为Role添加索引
        modelBuilder.Entity<Role>()
            .HasIndex(r => r.Name)
            .IsUnique();
        
        // 为Permission添加索引
        modelBuilder.Entity<Permission>()
            .HasIndex(p => p.Code)
            .IsUnique();
        
        // 为UserActivityLog添加索引
        modelBuilder.Entity<UserActivityLog>()
            .HasIndex(l => l.UserId);
        
        modelBuilder.Entity<UserActivityLog>()
            .HasIndex(l => l.ActivityType);
        
        modelBuilder.Entity<UserActivityLog>()
            .HasIndex(l => l.CreatedAt);
        
        // 配置Image实体
        modelBuilder.Entity<Image>()
            .ToTable("Images");
        
        modelBuilder.Entity<Image>()
            .HasOne(i => i.SpecialEquipment)
            .WithMany(e => e.Images)
            .HasForeignKey(i => i.SpecialEquipmentId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Image>()
            .HasOne(i => i.GeneralEquipment)
            .WithMany(e => e.Images)
            .HasForeignKey(i => i.GeneralEquipmentId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Image>()
            .HasIndex(i => i.SpecialEquipmentId);
        
        modelBuilder.Entity<Image>()
            .HasIndex(i => i.GeneralEquipmentId);
    }
}
