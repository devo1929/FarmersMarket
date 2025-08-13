namespace Core.Models;

public class AuthenticateRequestModel
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}