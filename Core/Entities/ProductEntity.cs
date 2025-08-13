using Core.Enums;

namespace Core.Entities;

public class ProductEntity
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public long VendorId { get; set; }
    public int Units { get; set; }
    public decimal PricePerUnit { get; set; }
    public ProductStatusEnum Status { get; set; }

    public virtual VendorEntity Vendor { get; set; }
    public virtual LocationEntity Location { get; set; } = null!;
}