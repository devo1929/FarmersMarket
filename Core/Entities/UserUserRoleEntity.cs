namespace Core.Entities;

public class UserUserRoleEntity
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long UserRoleId { get; set; }
    
    public virtual UserEntity User { get; set; } = null!;
    public virtual UserRoleEntity UserRole { get; set; } = null!;
}