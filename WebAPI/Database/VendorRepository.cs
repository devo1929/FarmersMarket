using Core.Entities;
using Core.Enums;
using Mock;

namespace WebAPI.Database;

public class VendorRepository
{
    public async Task<IEnumerable<VendorEntity>> GetAllAsync() =>
        MockData.Vendors.Where(v => v.Status == VendorStatusEnum.Active);
}