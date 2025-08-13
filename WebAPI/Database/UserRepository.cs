using Core.Entities;
using Mock;

namespace WebAPI.Database;

public class UserRepository
{
    public async Task<UserEntity> GetForEmailAsync(string email)
    {
        var user = MockData.Users.First();
        user.Email = email;
        return user;
    }
}