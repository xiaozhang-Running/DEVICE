using System;
using System.Collections.Generic;

namespace DeviceWarehouse.Domain.Entities
{
    public class Permission
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Code { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // 导航属性
        public virtual ICollection<Role> Roles { get; set; } = [];
    }
}
