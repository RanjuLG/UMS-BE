using BCrypt.Net;
using UMS_BE.Services.Interfaces;

namespace UMS_BE.Services.Implementations;

public class PasswordHashingService : IPasswordHashingService
{
    public string HashPassword(string password, out string salt)
    {
        salt = BCrypt.Net.BCrypt.GenerateSalt(12);
        return BCrypt.Net.BCrypt.HashPassword(password, salt);
    }

    public bool VerifyPassword(string password, string hash, string salt)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
