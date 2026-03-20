using System;
using System.Collections.Generic;

namespace DeviceWarehouse.Domain.Entities
{
    public class Role
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // 导航属性
        public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    }
}
