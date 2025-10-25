using UMS_BE.Models;
using UMS_BE.Repositories.Interfaces;
using UMS_BE.Services.Interfaces;

namespace UMS_BE.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHashingService _passwordHashingService;

    public UserService(IUserRepository userRepository, IPasswordHashingService passwordHashingService)
    {
        _userRepository = userRepository;
        _passwordHashingService = passwordHashingService;
    }

    public async Task<User?> ValidateUserAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null || !user.IsActive)
            return null;

        var isValid = _passwordHashingService.VerifyPassword(password, user.PasswordHash, user.PasswordSalt);
        return isValid ? user : null;
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
        return await _userRepository.GetByIdAsync(userId);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _userRepository.GetByEmailAsync(email);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _userRepository.GetAllAsync();
    }

    public async Task<User> CreateAsync(string userName, string firstName, string lastName, string email, string password, int platformId)
    {
        var hash = _passwordHashingService.HashPassword(password, out var salt);

        var user = new User
        {
            UserName = userName,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PasswordHash = hash,
            PasswordSalt = salt,
            PlatformId = platformId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        return await _userRepository.CreateAsync(user, null);
    }

    public async Task<User> UpdateAsync(int userId, string? userName, string? firstName, string? lastName, string? email, bool? isActive)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new ArgumentException("User not found");

        if (!string.IsNullOrEmpty(userName))
            user.UserName = userName;
        if (!string.IsNullOrEmpty(firstName))
            user.FirstName = firstName;
        if (!string.IsNullOrEmpty(lastName))
            user.LastName = lastName;
        if (!string.IsNullOrEmpty(email))
            user.Email = email;
        if (isActive.HasValue)
            user.IsActive = isActive.Value;

        return await _userRepository.UpdateAsync(user, userId);
    }

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return false;

        var isValid = _passwordHashingService.VerifyPassword(currentPassword, user.PasswordHash, user.PasswordSalt);
        if (!isValid)
            return false;

        var newHash = _passwordHashingService.HashPassword(newPassword, out var newSalt);
        user.PasswordHash = newHash;
        user.PasswordSalt = newSalt;

        await _userRepository.UpdateAsync(user, userId);
        return true;
    }

    public async Task<bool> DeleteAsync(int userId)
    {
        return await _userRepository.DeleteAsync(userId, userId);
    }
}
