namespace Core.Entities;

public class VendorUserEntity
{
    public long Id { get; set; }
    public long VendorId { get; set; }
    public long UserId { get; set; }
    
    public virtual VendorEntity Vendor { get; set; } = null!;
    public virtual UserEntity User { get; set; } = null!;
}