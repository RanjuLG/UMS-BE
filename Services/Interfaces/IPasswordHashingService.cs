namespace UMS_BE.Services.Interfaces;

public interface IPasswordHashingService
{
    string HashPassword(string password, out string salt);
    bool VerifyPassword(string password, string hash, string salt);
}
