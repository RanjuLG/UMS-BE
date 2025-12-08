using Microsoft.EntityFrameworkCore;
using UMS_BE.Data;
using UMS_BE.Models;

namespace UMS_BE.Data;

/// <summary>
/// Helper class to migrate existing users to the UserPlatforms table
/// Run this after applying the AddUserPlatformManyToMany migration
/// </summary>
public static class UserPlatformMigrationHelper
{
    public static async Task MigrateExistingUsersAsync(ApplicationDbContext context, int defaultPlatformId = 1)
    {
        // Get all users that don't have platform associations
        var usersWithoutPlatforms = await context.Users
            .Where(u => !context.UserPlatforms.Any(up => up.UserId == u.UserId))
            .ToListAsync();

        if (!usersWithoutPlatforms.Any())
        {
            Console.WriteLine("All users already have platform associations.");
            return;
        }

        Console.WriteLine($"Found {usersWithoutPlatforms.Count} users without platform associations.");

        // Check if the default platform exists
        var platform = await context.Platforms.FindAsync(defaultPlatformId);
        if (platform == null)
        {
            Console.WriteLine($"ERROR: Platform with ID {defaultPlatformId} not found!");
            return;
        }

        Console.WriteLine($"Associating users with platform: {platform.Name} (ID: {platform.PlatformId})");

        // Create UserPlatform associations
        foreach (var user in usersWithoutPlatforms)
        {
            var userPlatform = new UserPlatform
            {
                UserId = user.UserId,
                PlatformId = defaultPlatformId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = null
            };

            context.UserPlatforms.Add(userPlatform);
            Console.WriteLine($"  - Associated user {user.UserName} (ID: {user.UserId}) with platform {platform.Name}");
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"Successfully associated {usersWithoutPlatforms.Count} users with platform {platform.Name}");
    }

    /// <summary>
    /// Associate a specific user with multiple platforms
    /// </summary>
    public static async Task AssociateUserWithPlatformsAsync(
        ApplicationDbContext context, 
        int userId, 
        List<int> platformIds)
    {
        var user = await context.Users.FindAsync(userId);
        if (user == null)
        {
            Console.WriteLine($"ERROR: User with ID {userId} not found!");
            return;
        }

        // Remove existing associations
        var existingAssociations = await context.UserPlatforms
            .Where(up => up.UserId == userId)
            .ToListAsync();
        
        context.UserPlatforms.RemoveRange(existingAssociations);

        // Add new associations
        foreach (var platformId in platformIds)
        {
            var platform = await context.Platforms.FindAsync(platformId);
            if (platform == null)
            {
                Console.WriteLine($"WARNING: Platform with ID {platformId} not found! Skipping...");
                continue;
            }

            var userPlatform = new UserPlatform
            {
                UserId = userId,
                PlatformId = platformId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = null
            };

            context.UserPlatforms.Add(userPlatform);
            Console.WriteLine($"Associated user {user.UserName} with platform {platform.Name}");
        }

        await context.SaveChangesAsync();
    }
}
