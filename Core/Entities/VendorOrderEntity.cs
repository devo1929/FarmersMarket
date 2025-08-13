using Core.Enums;

namespace Core.Entities;

public class VendorOrderEntity
{
    public long Id { get; set; }
    public long VendorId { get; set; }
    public long OrderId { get; set; }
    public OrderStatusEnum Status { get; set; }
    public string? Notes { get; set; }
    
    public virtual VendorEntity Vendor { get; set; } = null!;
    public virtual OrderEntity Order { get; set; } = null!;
    
    public virtual ICollection<VendorOrderProductEntity> Products { get; set; } = new List<VendorOrderProductEntity>();
}