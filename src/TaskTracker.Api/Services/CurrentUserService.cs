using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TaskTracker.Api.Common;

namespace TaskTracker.Api.Services;

public interface ICurrentUserService { Guid UserId { get; } }

public sealed class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
{
    public Guid UserId
    {
        get
        {
            var value = accessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : throw new UnauthorizedException("A valid user identity is required.");
        }
    }
}
