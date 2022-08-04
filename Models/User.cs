using System.ComponentModel.DataAnnotations;
namespace Idea.Models;

public class User
{
    [Key]
    public string Email { get; set; } = "";
    public byte[] PasswordHash { get; set; } = new byte[32];
    public byte[] PasswordSalt { get; set; } = new byte[32];
    public bool Verified { get; set; } = false;
    public string? VerificationToken { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpires { get; set; }
    public string Role { get; set; } = "User";
}

public class UserLogin
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public class UserRegister
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";

    //Add other properties
}

public class UserResetPassword
{
    public string PasswordResetToken { get; set; } = "";
    public string Password { get; set; } = "";
}