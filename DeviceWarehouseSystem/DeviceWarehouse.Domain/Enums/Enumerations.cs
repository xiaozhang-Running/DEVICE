using System.ComponentModel;

namespace DeviceWarehouse.Domain.Enums;

public enum DeviceType
{
    [Description("专用设备")]
    SpecialDevice = 1,
    
    [Description("通用设备")]
    GeneralDevice = 2,
    
    [Description("耗材")]
    Consumable = 3,
    
    [Description("原材料")]
    RawMaterial = 4
}

public enum DeviceStatus
{
    [Description("正常")]
    Normal = 1,

    [Description("损坏")]
    Broken = 2,

    [Description("报废")]
    Scrap = 3
}

public enum UsageStatus
{
    [Description("未使用")]
    Unused = 0,
    
    [Description("使用中")]
    InUse = 1
}

public enum InboundType
{
    [Description("采购入库")]
    Purchase = 1,
    
    [Description("项目入库")]
    Project = 2
}

public enum OutboundType
{
    [Description("项目出库")]
    Project = 1,
    
    [Description("耗材出库")]
    Consumable = 2
}

public enum OrderStatus
{
    [Description("待处理")]
    Pending = 0,
    
    [Description("已完成")]
    Completed = 1
}

public enum LogisticsMethod
{
    [Description("随身携带")]
    CarryOn = 1,
    
    [Description("跨越物流")]
    Kuayue = 2,
    
    [Description("德邦物流")]
    Deppon = 3,
    
    [Description("顺丰物流")]
    SFExpress = 4,
    
    [Description("其他")]
    Other = 5
}
