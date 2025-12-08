using UMS_BE.Models;

namespace UMS_BE.Services.Interfaces;

public interface IUserService
{
    Task<User?> ValidateUserAsync(string email, string password);
    Task<User?> GetByIdAsync(int userId);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllAsync();
    Task<User> CreateAsync(string userName, string firstName, string lastName, string email, string password, List<int> platformIds);
    Task<User> UpdateAsync(int userId, string? userName, string? firstName, string? lastName, string? email, bool? isActive);
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    Task<bool> DeleteAsync(int userId);
}
