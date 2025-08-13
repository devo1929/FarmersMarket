namespace Core.Entities;

public class VendorUserUserRoleEntity
{
    public long Id { get; set; }
    public long VendorUserId { get; set; }
    public long VendorUserRoleId { get; set; }
    
    public virtual VendorUserEntity VendorUser { get; set; } = null!;
    public virtual VendorUserRoleEntity VendorUserRole { get; set; } = null!;
}