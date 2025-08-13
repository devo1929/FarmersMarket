namespace Core.Entities;

public class VendorLocationEntity
{
    public long Id { get; set; }
    public long VendorId { get; set; }
    public long LocationId { get; set; }
    
    public virtual VendorEntity Vendor { get; set; } = null!;
    public virtual LocationEntity Location { get; set; } = null!;
}