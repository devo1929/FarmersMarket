using Core.Enums;

namespace Core.Entities;

public class VendorOrderProductEntity
{
    public long Id { get; set; }
    public long VendorOrderId { get; set; }
    public long ProductId { get; set; }
    public int Units { get; set; }
    public VendorOrderProductStatusEnum Status { get; set; }
    public string? Notes { get; set; }

    // public virtual VendorOrderEntity VendorOrder { get; set; } = null!;
    public virtual ProductEntity Product { get; set; } = null!;
}