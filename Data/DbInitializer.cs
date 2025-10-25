using Microsoft.EntityFrameworkCore;
using UMS_BE.Data;
using UMS_BE.Models;
using UMS_BE.Services.Interfaces;

namespace UMS_BE.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context, IPasswordHashingService passwordHashingService)
    {
        // Ensure database is created
        await context.Database.MigrateAsync();

        // Check if data already exists
        if (await context.Users.AnyAsync())
        {
            return; // Database has been seeded
        }

        // Create Admin Role
        var adminRole = new Role
        {
            Name = "Admin",
            Description = "System Administrator with full access",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Create User Role
        var userRole = new Role
        {
            Name = "User",
            Description = "Standard user with limited access",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Roles.AddRange(adminRole, userRole);
        await context.SaveChangesAsync(); // Save to generate RoleIds

        // Create Admin User
        var adminPassword = "Admin@123"; // Change this in production!
        var adminHash = passwordHashingService.HashPassword(adminPassword, out var adminSalt);

        var adminUser = new User
        {
            UserName = "admin",
            FirstName = "System",
            LastName = "Administrator",
            Email = "admin@ums.com",
            PasswordHash = adminHash,
            PasswordSalt = adminSalt,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(adminUser);
        await context.SaveChangesAsync(); // Save to generate UserId

        // Assign Admin Role to Admin User
        var userAdminRole = new UserRole
        {
            UserId = adminUser.UserId,
            RoleId = adminRole.RoleId,
            AssignedAt = DateTime.UtcNow
        };

        context.UserRoles.Add(userAdminRole);

        // Create Demo Platform
        var demoPlatform = new Platform
        {
            Name = "Demo Platform",
            ClientId = "demo_platform",
            ClientSecret = "demo_secret_change_in_production",
            Description = "Demo platform for testing",
            RedirectUris = System.Text.Json.JsonSerializer.Serialize(new List<string> { "https://localhost:3000/callback" }),
            PostLogoutRedirectUris = System.Text.Json.JsonSerializer.Serialize(new List<string> { "https://localhost:3000" }),
            AllowedScopes = System.Text.Json.JsonSerializer.Serialize(new List<string> { "openid", "profile", "email", "roles", "permissions", "api" }),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Platforms.Add(demoPlatform);
        await context.SaveChangesAsync(); // Save to generate PlatformId

        // Create Demo Permissions
        var permissions = new[]
        {
            new Permission
            {
                Name = "View Users",
                Description = "Can view user list",
                Resource = "users",
                Action = "read",
                PlatformId = demoPlatform.PlatformId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Permission
            {
                Name = "Create Users",
                Description = "Can create new users",
                Resource = "users",
                Action = "create",
                PlatformId = demoPlatform.PlatformId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Permission
            {
                Name = "Update Users",
                Description = "Can update existing users",
                Resource = "users",
                Action = "update",
                PlatformId = demoPlatform.PlatformId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Permission
            {
                Name = "Delete Users",
                Description = "Can delete users",
                Resource = "users",
                Action = "delete",
                PlatformId = demoPlatform.PlatformId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Permissions.AddRange(permissions);
        await context.SaveChangesAsync(); // Save to generate PermissionIds

        // Assign all permissions to Admin Role
        foreach (var permission in permissions)
        {
            context.RolePermissions.Add(new RolePermission
            {
                RoleId = adminRole.RoleId,
                PermissionId = permission.PermissionId,
                AssignedAt = DateTime.UtcNow
            });
        }

        // Assign read permission to User Role
        context.RolePermissions.Add(new RolePermission
        {
            RoleId = userRole.RoleId,
            PermissionId = permissions[0].PermissionId,
            AssignedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        Console.WriteLine("Database seeded successfully!");
        Console.WriteLine($"Admin User - Email: admin@ums.com, Password: {adminPassword}");
        Console.WriteLine($"Demo Platform - ClientId: demo_platform, ClientSecret: demo_secret_change_in_production");
    }
}
