using Core.Entities;
using Core.Models;
using WebAPI.Database;

namespace WebAPI.Services;

public class AuthenticateService(UserRepository userRepository, VendorRepository vendorRepository, TokenService tokenService)
{
    public async Task<AuthenticateResponseModel> AuthenticateAsync(AuthenticateRequestModel authenticateRequestModel)
    {
        // just randomly grab a user and vendor as mock for now
        var user = await userRepository.GetForEmailAsync(authenticateRequestModel.Email);
        var vendor = (await vendorRepository.GetAllAsync()).First();
        return new AuthenticateResponseModel
        {
            Token = tokenService.GenerateToken(new VendorUserEntity
            {
                User = user,
                Vendor = vendor
            })
        };
    }
}