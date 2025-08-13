namespace Core.Entities;

public class UserEntity
{
    public long Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public byte[] Hash { get; set; } = null!;
    public byte[] Salt { get; set; } = null!;
    
    public virtual ICollection<VendorUserEntity> VendorUsers { get; set; } = null!;
}