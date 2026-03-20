using System;

namespace DeviceWarehouse.Domain.Entities
{
    public class UserActivityLog
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string ActivityType { get; set; } = string.Empty; // Login, Logout, Create, Update, Delete, etc.
    public string ActivityDescription { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    // 导航属性
    public virtual User User { get; set; } = null!;
}
}
