using AutoMapper;
using Core.Models;
using WebAPI.Database;

namespace WebAPI.Services;

public class VendorService(IMapper mapper, VendorRepository vendorRepository)
{
    public async Task<IEnumerable<VendorModel>> GetAllAsync() =>
        mapper.Map<IEnumerable<VendorModel>>(await vendorRepository.GetAllAsync());
}