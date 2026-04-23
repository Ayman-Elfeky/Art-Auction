using ArtAuction.Domain.Enums;
using System.Security.Claims;

namespace Api.Services;

public interface ICurrentUserContext
{
    Guid GetRequiredUserId();
    bool IsAuthenticated();
    bool IsInRole(UserRole role);
}

public sealed class CurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetRequiredUserId()
    {
        var principal = _httpContextAccessor.HttpContext?.User
            ?? throw new InvalidOperationException("No HTTP context found.");

        var sub = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub");

        if (!Guid.TryParse(sub, out var userId))
        {
            throw new InvalidOperationException("Invalid or missing authenticated user id.");
        }

        return userId;
    }

    public bool IsAuthenticated()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
    }

    public bool IsInRole(UserRole role)
    {
        return _httpContextAccessor.HttpContext?.User?.IsInRole(role.ToString()) == true;
    }
}
