namespace UMS_BE.Models;

public class UserPlatform
{
    public int UserId { get; set; }
    public int PlatformId { get; set; }
    
    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedBy { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Platform Platform { get; set; } = null!;
}
