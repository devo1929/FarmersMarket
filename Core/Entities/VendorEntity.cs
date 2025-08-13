using Core.Enums;

namespace Core.Entities;

public class VendorEntity
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public VendorStatusEnum Status { get; set; }

    public virtual ICollection<VendorLocationEntity> VendorLocations { get; set; } = new List<VendorLocationEntity>();
    public virtual ICollection<VendorUserEntity> VendorUsers { get; set; } = new List<VendorUserEntity>();
}