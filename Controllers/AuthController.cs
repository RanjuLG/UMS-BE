using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UMS_BE.Data;
using UMS_BE.Models;
using UMS_BE.Models.DTOs;
using UMS_BE.Repositories.Interfaces;
using UMS_BE.Services.Interfaces;
using FirebaseAdmin.Auth;

namespace UMS_BE.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;
    private readonly IPlatformRepository _platformRepository;
    private readonly IUserRepository _userRepository;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUserService userService,
        ITokenService tokenService,
        IPlatformRepository platformRepository,
        IUserRepository userRepository,
        ApplicationDbContext context,
        ILogger<AuthController> logger)
    {
        _userService = userService;
        _tokenService = tokenService;
        _platformRepository = platformRepository;
        _userRepository = userRepository;
        _context = context;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            // Validate client credentials if provided
            if (!string.IsNullOrEmpty(request.ClientId))
            {
                if (string.IsNullOrEmpty(request.ClientSecret))
                {
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Client secret is required when client ID is provided"
                    });
                }

                var platform = await _platformRepository.GetByClientIdAsync(request.ClientId);
                if (platform == null || !platform.IsActive || platform.ClientSecret != request.ClientSecret)
                {
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid client credentials"
                    });
                }
            }

            // Validate user credentials
            var user = await _userService.ValidateUserAsync(request.Email, request.Password);
            if (user == null)
            {
                return Unauthorized(new LoginResponse
                {
                    Success = false,
                    Message = "Invalid email or password"
                });
            }

            // Generate tokens
            var accessToken = await _tokenService.GenerateAccessTokenAsync(user, request.ClientId);
            var refreshToken = await _tokenService.GenerateRefreshToken();

            // Store refresh token
            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.UserId,
                ClientId = request.ClientId,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            return Ok(new LoginResponse
            {
                Success = true,
                Message = "Login successful",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = 3600, // 1 hour
                User = new UserDto
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    IsActive = user.IsActive,
                    Platforms = user.UserPlatforms.Select(up => new PlatformDto
                    {
                        PlatformId = up.Platform.PlatformId,
                        Name = up.Platform.Name
                    }).ToList(),
                    Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new LoginResponse
            {
                Success = false,
                Message = "An error occurred during login"
            });
        }
    }

    /// <summary>
    /// External login using Firebase/Google token
    /// Accepts a Firebase ID token, validates it, and returns system tokens
    /// </summary>
    [HttpPost("external-login")]
    public async Task<ActionResult<ExternalLoginResponse>> ExternalLogin([FromBody] ExternalLoginRequest request)
    {
        try
        {
            // Validate ClientId if provided (same pattern as normal login)
            if (!string.IsNullOrEmpty(request.ClientId))
            {
                var platform = await _platformRepository.GetByClientIdAsync(request.ClientId);
                if (platform == null || !platform.IsActive)
                {
                    return Unauthorized(new ExternalLoginResponse
                    {
                        Success = false,
                        Message = "Invalid or inactive client application"
                    });
                }
            }

            // Verify Firebase token - this is the proof of identity
            FirebaseToken decodedToken;
            try
            {
                decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.FirebaseToken);
            }
            catch (FirebaseAuthException ex)
            {
                _logger.LogWarning(ex, "Invalid Firebase token");
                return Unauthorized(new ExternalLoginResponse
                {
                    Success = false,
                    Message = "Invalid or expired Firebase token"
                });
            }

            // Extract user info from Firebase token
            var firebaseUid = decodedToken.Uid;
            var email = decodedToken.Claims.TryGetValue("email", out var emailClaim) ? emailClaim?.ToString() : null;
            var name = decodedToken.Claims.TryGetValue("name", out var nameClaim) ? nameClaim?.ToString() : null;
            var picture = decodedToken.Claims.TryGetValue("picture", out var pictureClaim) ? pictureClaim?.ToString() : null;

            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new ExternalLoginResponse
                {
                    Success = false,
                    Message = "Email not found in Firebase token"
                });
            }

            // Check if user exists
            var user = await _userService.GetByEmailAsync(email);
            var isNewUser = false;

            if (user == null)
            {
                // Create new user from Firebase data
                isNewUser = true;
                var firstName = name?.Split(' ').FirstOrDefault() ?? email.Split('@')[0];
                var lastName = name?.Split(' ').Skip(1).FirstOrDefault() ?? "";
                var userName = email.Split('@')[0];

                // Generate a random password for external users (they won't use it)
                var randomPassword = Guid.NewGuid().ToString("N");

                user = await _userService.CreateAsync(
                    userName,
                    firstName,
                    lastName,
                    email,
                    randomPassword,
                    new List<int>()); // No platforms initially

                // Reload user with navigation properties
                user = await _userService.GetByIdAsync(user.UserId);
            }

            if (user == null || !user.IsActive)
            {
                return Unauthorized(new ExternalLoginResponse
                {
                    Success = false,
                    Message = "User account is inactive"
                });
            }

            // Generate system tokens
            var accessToken = await _tokenService.GenerateAccessTokenAsync(user, request.ClientId);
            var refreshToken = await _tokenService.GenerateRefreshToken();

            // Store refresh token
            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.UserId,
                ClientId = request.ClientId,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            return Ok(new ExternalLoginResponse
            {
                Success = true,
                Message = isNewUser ? "User registered and logged in successfully" : "Login successful",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = 3600, // 1 hour
                IsNewUser = isNewUser,
                User = new UserDto
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    IsActive = user.IsActive,
                    Platforms = user.UserPlatforms.Select(up => new PlatformDto
                    {
                        PlatformId = up.Platform.PlatformId,
                        Name = up.Platform.Name
                    }).ToList(),
                    Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during external login");
            return StatusCode(500, new ExternalLoginResponse
            {
                Success = false,
                Message = "An error occurred during external login"
            });
        }
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<TokenResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var refreshTokenEntity = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

            if (refreshTokenEntity == null || !refreshTokenEntity.IsActive)
            {
                return Unauthorized(new { message = "Invalid or expired refresh token" });
            }

            // Validate client if provided
            if (!string.IsNullOrEmpty(request.ClientId) && refreshTokenEntity.ClientId != request.ClientId)
            {
                return Unauthorized(new { message = "Client ID mismatch" });
            }

            // Generate new tokens
            var accessToken = await _tokenService.GenerateAccessTokenAsync(
                refreshTokenEntity.User,
                refreshTokenEntity.ClientId);
            var newRefreshToken = await _tokenService.GenerateRefreshToken();

            // Revoke old refresh token
            refreshTokenEntity.RevokedAt = DateTime.UtcNow;

            // Store new refresh token
            var newRefreshTokenEntity = new RefreshToken
            {
                Token = newRefreshToken,
                UserId = refreshTokenEntity.UserId,
                ClientId = refreshTokenEntity.ClientId,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(newRefreshTokenEntity);
            await _context.SaveChangesAsync();

            return Ok(new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = 3600
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, new { message = "An error occurred during token refresh" });
        }
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest? request)
    {
        try
        {
            if (request != null && !string.IsNullOrEmpty(request.RefreshToken))
            {
                var refreshToken = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

                if (refreshToken != null && refreshToken.IsActive)
                {
                    refreshToken.RevokedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }

            return Ok(new { message = "Logout successful" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new { message = "An error occurred during logout" });
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var existingUser = await _userService.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Email already exists" });
            }

            var user = await _userService.CreateAsync(
                request.UserName,
                request.FirstName,
                request.LastName,
                request.Email,
                request.Password,
                request.PlatformIds);

            // Reload user with platforms
            user = await _userService.GetByIdAsync(user.UserId);

            return CreatedAtAction(nameof(Register), new UserDto
            {
                UserId = user!.UserId,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                IsActive = user.IsActive,
                Platforms = user.UserPlatforms.Select(up => new PlatformDto
                {
                    PlatformId = up.Platform.PlatformId,
                    Name = up.Platform.Name
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpPost("revoke-all")]
    public async Task<IActionResult> RevokeAllTokens()
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid user" });
            }

            var tokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "All refresh tokens revoked successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking tokens");
            return StatusCode(500, new { message = "An error occurred while revoking tokens" });
        }
    }

    /// <summary>
    /// Validates user's access to a specific platform and optionally generates a platform-specific token
    /// Use this when user clicks on a third-party platform from UMS
    /// </summary>
    [Authorize]
    [HttpPost("validate-platform-access")]
    public async Task<ActionResult<PlatformAccessResponse>> ValidatePlatformAccess([FromBody] PlatformAccessRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new PlatformAccessResponse
                {
                    Success = false,
                    Message = "Invalid user",
                    HasAccess = false
                });
            }

            // Get user's platforms
            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new PlatformAccessResponse
                {
                    Success = false,
                    Message = "User not found",
                    HasAccess = false
                });
            }

            // Check if user has access to this platform
            var userPlatform = user.UserPlatforms.FirstOrDefault(up => up.PlatformId == request.PlatformId);
            if (userPlatform == null)
            {
                return Ok(new PlatformAccessResponse
                {
                    Success = true,
                    Message = "User does not have access to this platform",
                    HasAccess = false
                });
            }

            // Get platform details
            var platform = await _platformRepository.GetByIdAsync(request.PlatformId);
            if (platform == null || !platform.IsActive)
            {
                return Ok(new PlatformAccessResponse
                {
                    Success = true,
                    Message = "Platform not found or inactive",
                    HasAccess = false
                });
            }

            // Validate client credentials if provided
            if (!string.IsNullOrEmpty(request.ClientId) && platform.ClientId != request.ClientId)
            {
                return Ok(new PlatformAccessResponse
                {
                    Success = true,
                    Message = "Invalid client ID",
                    HasAccess = false
                });
            }

            // Get user's roles and permissions for this platform
            var roles = await _userRepository.GetUserRolesAsync(userId);
            var permissions = await _userRepository.GetUserPermissionsForPlatformAsync(userId, request.PlatformId);

            // Parse redirect URIs
            var redirectUris = string.IsNullOrEmpty(platform.RedirectUris) 
                ? Array.Empty<string>() 
                : System.Text.Json.JsonSerializer.Deserialize<string[]>(platform.RedirectUris) ?? Array.Empty<string>();

            var postLogoutRedirectUris = string.IsNullOrEmpty(platform.PostLogoutRedirectUris) 
                ? Array.Empty<string>() 
                : System.Text.Json.JsonSerializer.Deserialize<string[]>(platform.PostLogoutRedirectUris) ?? Array.Empty<string>();

            var allowedScopes = string.IsNullOrEmpty(platform.AllowedScopes) 
                ? Array.Empty<string>() 
                : System.Text.Json.JsonSerializer.Deserialize<string[]>(platform.AllowedScopes) ?? Array.Empty<string>();

            // Generate platform-specific token with clientId
            var platformToken = await _tokenService.GenerateAccessTokenAsync(user, platform.ClientId);

            return Ok(new PlatformAccessResponse
            {
                Success = true,
                Message = "User has access to this platform",
                HasAccess = true,
                Platform = new PlatformAccessDetails
                {
                    PlatformId = platform.PlatformId,
                    Name = platform.Name,
                    ClientId = platform.ClientId,
                    RedirectUris = redirectUris,
                    PostLogoutRedirectUris = postLogoutRedirectUris,
                    AllowedScopes = allowedScopes,
                    Permissions = permissions.Select(p => $"{p.Resource}:{p.Action}").ToList(),
                    Roles = roles.Select(r => r.Name).ToList()
                },
                PlatformSpecificToken = platformToken
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating platform access");
            return StatusCode(500, new PlatformAccessResponse
            {
                Success = false,
                Message = "An error occurred while validating platform access",
                HasAccess = false
            });
        }
    }

    /// <summary>
    /// Get redirect URL for a platform with authentication token
    /// Use this to generate the complete redirect URL with token
    /// </summary>
    [Authorize]
    [HttpGet("platform-redirect/{platformId}")]
    public async Task<ActionResult<PlatformRedirectResponse>> GetPlatformRedirectUrl(int platformId)
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid user" });
            }

            // Validate access
            var accessResult = await ValidatePlatformAccess(new PlatformAccessRequest { PlatformId = platformId });
            if (accessResult.Result is OkObjectResult okResult && okResult.Value is PlatformAccessResponse response)
            {
                if (!response.HasAccess || response.Platform == null)
                {
                    return Forbid();
                }

                // Get the first redirect URI
                var redirectUri = response.Platform.RedirectUris.FirstOrDefault();
                if (string.IsNullOrEmpty(redirectUri))
                {
                    return BadRequest(new { message = "No redirect URI configured for this platform" });
                }

                // Build redirect URL with token
                var separator = redirectUri.Contains('?') ? "&" : "?";
                var redirectUrl = $"{redirectUri}{separator}token={response.PlatformSpecificToken}";

                return Ok(new PlatformRedirectResponse
                {
                    RedirectUrl = redirectUrl,
                    Token = response.PlatformSpecificToken!,
                    Platform = response.Platform
                });
            }

            return StatusCode(500, new { message = "Failed to validate platform access" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating platform redirect URL");
            return StatusCode(500, new { message = "An error occurred while generating redirect URL" });
        }
    }
}
