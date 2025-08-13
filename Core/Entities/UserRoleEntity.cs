using Core.Enums;

namespace Core.Entities;

public class UserRoleEntity
{
    public UserRoleEnum Id { get; set; }
    public string Name { get; set; } = null!;
}