using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;
using UMS_BE.Services.Interfaces;

namespace UMS_BE.IdentityServer;

public class CustomPasswordValidator : IResourceOwnerPasswordValidator
{
    private readonly IUserService _userService;

    public CustomPasswordValidator(IUserService userService)
    {
        _userService = userService;
    }

    public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
    {
        var user = await _userService.ValidateUserAsync(context.UserName, context.Password);

        if (user != null)
        {
            context.Result = new GrantValidationResult(
                subject: user.UserId.ToString(),
                authenticationMethod: "password");
        }
        else
        {
            context.Result = new GrantValidationResult(
                TokenRequestErrors.InvalidGrant,
                "Invalid username or password");
        }
    }
}
