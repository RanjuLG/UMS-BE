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
        if (await context.Platforms.AnyAsync())
        {
            return; // Database has been seeded
        }

        // Create UMS Platform (the system itself)
        var umsPlatform = new Platform
        {
            Name = "UMS",
            ClientId = "ums_platform",
            ClientSecret = "ums_secret_change_in_production",
            Description = "User Management System - The main platform",
            RedirectUris = System.Text.Json.JsonSerializer.Serialize(new List<string> { "https://localhost:5001/callback" }),
            PostLogoutRedirectUris = System.Text.Json.JsonSerializer.Serialize(new List<string> { "https://localhost:5001" }),
            AllowedScopes = System.Text.Json.JsonSerializer.Serialize(new List<string> { "openid", "profile", "email", "roles", "permissions", "api" }),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

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

        context.Platforms.AddRange(umsPlatform, demoPlatform);
        await context.SaveChangesAsync(); // Save to generate PlatformIds

        // Create Admin Roles for both platforms
        var umsAdminRole = new Role
        {
            Name = "Admin",
            Description = "System Administrator with full access to UMS",
            PlatformId = umsPlatform.PlatformId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var umsUserRole = new Role
        {
            Name = "User",
            Description = "Standard user with limited access to UMS",
            PlatformId = umsPlatform.PlatformId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var demoAdminRole = new Role
        {
            Name = "Admin",
            Description = "Administrator role for Demo Platform",
            PlatformId = demoPlatform.PlatformId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var demoUserRole = new Role
        {
            Name = "User",
            Description = "Standard user role for Demo Platform",
            PlatformId = demoPlatform.PlatformId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Roles.AddRange(umsAdminRole, umsUserRole, demoAdminRole, demoUserRole);
        await context.SaveChangesAsync(); // Save to generate RoleIds

        // Create Admin User for UMS
        var adminPassword = "Admin@123"; // Change this in production!
        var adminHash = passwordHashingService.HashPassword(adminPassword, out var adminSalt);

        var umsAdminUser = new User
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

        // Create Demo User for Demo Platform
        var demoUserPassword = "Demo@123";
        var demoHash = passwordHashingService.HashPassword(demoUserPassword, out var demoSalt);

        var demoUser = new User
        {
            UserName = "demo",
            FirstName = "Demo",
            LastName = "User",
            Email = "demo@demo.com",
            PasswordHash = demoHash,
            PasswordSalt = demoSalt,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.AddRange(umsAdminUser, demoUser);
        await context.SaveChangesAsync(); // Save to generate UserIds

        // Assign platforms to users
        var umsAdminUserPlatform = new UserPlatform
        {
            UserId = umsAdminUser.UserId,
            PlatformId = umsPlatform.PlatformId,
            CreatedAt = DateTime.UtcNow
        };

        var demoUserPlatform = new UserPlatform
        {
            UserId = demoUser.UserId,
            PlatformId = demoPlatform.PlatformId,
            CreatedAt = DateTime.UtcNow
        };

        context.UserPlatforms.AddRange(umsAdminUserPlatform, demoUserPlatform);
        await context.SaveChangesAsync();

        // Assign Roles to Users
        var umsAdminUserRole = new UserRole
        {
            UserId = umsAdminUser.UserId,
            RoleId = umsAdminRole.RoleId,
            AssignedAt = DateTime.UtcNow
        };

        var demoUserUserRole = new UserRole
        {
            UserId = demoUser.UserId,
            RoleId = demoUserRole.RoleId,
            AssignedAt = DateTime.UtcNow
        };

        context.UserRoles.AddRange(umsAdminUserRole, demoUserUserRole);

        // Create UMS Permissions
        var umsPermissions = new[]
        {
            new Permission
            {
                Name = "Manage Platforms",
                Description = "Can manage all platforms",
                Resource = "platforms",
                Action = "manage",
                PlatformId = umsPlatform.PlatformId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Permission
            {
                Name = "Manage Users",
                Description = "Can manage all users",
                Resource = "users",
                Action = "manage",
                PlatformId = umsPlatform.PlatformId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Permission
            {
                Name = "Manage Roles",
                Description = "Can manage all roles",
                Resource = "roles",
                Action = "manage",
                PlatformId = umsPlatform.PlatformId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Permission
            {
                Name = "View Users",
                Description = "Can view user list",
                Resource = "users",
                Action = "read",
                PlatformId = umsPlatform.PlatformId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        // Create Demo Platform Permissions
        var demoPermissions = new[]
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

        context.Permissions.AddRange(umsPermissions);
        context.Permissions.AddRange(demoPermissions);
        await context.SaveChangesAsync(); // Save to generate PermissionIds

        // Assign all UMS permissions to UMS Admin Role
        foreach (var permission in umsPermissions)
        {
            context.RolePermissions.Add(new RolePermission
            {
                RoleId = umsAdminRole.RoleId,
                PermissionId = permission.PermissionId,
                AssignedAt = DateTime.UtcNow
            });
        }

        // Assign view permission to UMS User Role
        context.RolePermissions.Add(new RolePermission
        {
            RoleId = umsUserRole.RoleId,
            PermissionId = umsPermissions[3].PermissionId, // View Users permission
            AssignedAt = DateTime.UtcNow
        });

        // Assign all demo permissions to Demo Admin Role
        foreach (var permission in demoPermissions)
        {
            context.RolePermissions.Add(new RolePermission
            {
                RoleId = demoAdminRole.RoleId,
                PermissionId = permission.PermissionId,
                AssignedAt = DateTime.UtcNow
            });
        }

        // Assign read permission to Demo User Role
        context.RolePermissions.Add(new RolePermission
        {
            RoleId = demoUserRole.RoleId,
            PermissionId = demoPermissions[0].PermissionId, // View Users permission
            AssignedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        Console.WriteLine("Database seeded successfully!");
        Console.WriteLine("=== UMS Platform ===");
        Console.WriteLine($"Platform: {umsPlatform.Name}");
        Console.WriteLine($"ClientId: {umsPlatform.ClientId}");
        Console.WriteLine($"Admin User - Email: admin@ums.com, Password: {adminPassword}");
        Console.WriteLine();
        Console.WriteLine("=== Demo Platform ===");
        Console.WriteLine($"Platform: {demoPlatform.Name}");
        Console.WriteLine($"ClientId: {demoPlatform.ClientId}");
        Console.WriteLine($"Demo User - Email: demo@demo.com, Password: {demoUserPassword}");
    }
}
