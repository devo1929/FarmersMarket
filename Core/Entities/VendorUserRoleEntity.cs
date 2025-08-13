using Core.Enums;

namespace Core.Entities;

public class VendorUserRoleEntity
{
    public VendorUserRoleEnum Id { get; set; }
    public string Name { get; set; } = null!;
}