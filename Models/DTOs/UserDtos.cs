using System.ComponentModel.DataAnnotations;

namespace UMS_BE.Models.DTOs;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
}

public class LoginResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public int ExpiresIn { get; set; }
    public UserDto? User { get; set; }
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
    public string? ClientId { get; set; }
}

public class TokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
}

public class RegisterRequest
{
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public List<int> PlatformIds { get; set; } = new();
}

public class UserDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<PlatformDto> Platforms { get; set; } = new();
    public List<string> Roles { get; set; } = new();
}

public class CreateUserRequest
{
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public List<int> PlatformIds { get; set; } = new();
}

public class UpdateUserRequest
{
    public string? UserName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public bool? IsActive { get; set; }
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>
/// Request for swapping a Firebase/Google token for a UMS System Token.
/// </summary>
public class ExternalLoginRequest
{
    /// <summary>
    /// The raw ID Token received from Firebase/Google Identity on the frontend.
    /// </summary>
    [Required(ErrorMessage = "The Firebase Token is required.")]
    public string FirebaseToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional: The ID of the application the user is trying to access (e.g., "sub-pal-web").
    /// If not provided, user logs directly into UMS.
    /// </summary>
    public string? ClientId { get; set; }
}

/// <summary>
/// Response for external login
/// </summary>
public class ExternalLoginResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public int ExpiresIn { get; set; }
    public UserDto? User { get; set; }
    public bool IsNewUser { get; set; }
}
