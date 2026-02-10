using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FamilyVault.API.EndPoints;

/// <summary>
/// Represents Helper.
/// </summary>
public static class Helper
{
    /// <summary>
    /// Performs the GetUserIdFromClaims operation.
    /// </summary>
    public static Guid GetUserIdFromClaims(ClaimsPrincipal user)
    {
        var userIdClaim = user.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
       
        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user claims.");
        }
        return userId;
    }
}
