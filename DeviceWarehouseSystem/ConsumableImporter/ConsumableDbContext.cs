using Microsoft.EntityFrameworkCore;

namespace ConsumableImporter;

public class ConsumableDbContext : DbContext
{
    public DbSet<ConsumableEntity> Consumables { get; set; } = null!;
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=localhost;Database=DeviceWarehouse;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;Pooling=true;Min Pool Size=5;Max Pool Size=100;Connection Timeout=30;");
    }
}

public class ConsumableEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? ModelSpecification { get; set; }
    public int TotalQuantity { get; set; }
    public int UsedQuantity { get; set; }
    public int RemainingQuantity { get; set; }
    public string? Unit { get; set; }
    public string? Company { get; set; }
    public string? Location { get; set; }
    public string? Remark { get; set; }
    public string? Image { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}