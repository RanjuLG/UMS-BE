using Microsoft.EntityFrameworkCore;
using UMS_BE.Models;

namespace UMS_BE.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<Platform> Platforms { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<UserPlatform> UserPlatforms { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId).ValueGeneratedOnAdd(); // Identity column
            // Email and UserName should be unique
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.UserName).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.UserName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.PasswordSalt).IsRequired();
            
            // Soft delete query filter
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        // Role configuration
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId);
            entity.Property(e => e.RoleId).ValueGeneratedOnAdd(); // Identity column
            // Role name should be unique per platform
            entity.HasIndex(e => new { e.Name, e.PlatformId }).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            
            // Platform relationship
            entity.HasOne(e => e.Platform)
                .WithMany(p => p.Roles)
                .HasForeignKey(e => e.PlatformId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Soft delete query filter
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        // Permission configuration
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.PermissionId);
            entity.Property(e => e.PermissionId).ValueGeneratedOnAdd(); // Identity column
            entity.HasIndex(e => new { e.Name, e.PlatformId }).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Resource).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(50);

            entity.HasOne(e => e.Platform)
                .WithMany(p => p.Permissions)
                .HasForeignKey(e => e.PlatformId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Soft delete query filter
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        // Platform configuration
        modelBuilder.Entity<Platform>(entity =>
        {
            entity.HasKey(e => e.PlatformId);
            entity.Property(e => e.PlatformId).ValueGeneratedOnAdd(); // Identity column
            entity.HasIndex(e => e.ClientId).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ClientId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ClientSecret).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            
            // Soft delete query filter
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        // UserRole configuration (many-to-many)
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId });

            entity.HasOne(e => e.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UserPlatform configuration (many-to-many)
        modelBuilder.Entity<UserPlatform>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.PlatformId });

            entity.HasOne(e => e.User)
                .WithMany(u => u.UserPlatforms)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Platform)
                .WithMany(p => p.UserPlatforms)
                .HasForeignKey(e => e.PlatformId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // RolePermission configuration (many-to-many)
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => new { e.RoleId, e.PermissionId });

            entity.HasOne(e => e.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // RefreshToken configuration
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.RefreshTokenId);
            entity.Property(e => e.RefreshTokenId).ValueGeneratedOnAdd();
            entity.HasIndex(e => e.Token).IsUnique();
            entity.Property(e => e.Token).IsRequired();
            entity.Property(e => e.ClientId).HasMaxLength(100);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
