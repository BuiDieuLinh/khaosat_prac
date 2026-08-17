using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

public abstract class BaseController : Controller
{
    protected Guid? CurrentUserId
    {
        get
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userId, out var id) ? id : null;
        }
    }

    protected string? CurrentUserName => User.FindFirst(ClaimTypes.Name)?.Value;

    protected string? CurrentUserEmail => User.FindFirst(ClaimTypes.Email)?.Value;

    protected string? CurrentUserRole => User.FindFirst(ClaimTypes.Role)?.Value;

    protected string? GetClientIpAddress()
    {
        if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor) && !string.IsNullOrWhiteSpace(forwardedFor))
        {
            return forwardedFor.ToString().Split(',')[0].Trim();
        }
        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    protected string? GetUserAgent()
    {
        if (Request.Headers.TryGetValue("User-Agent", out var userAgent) && !string.IsNullOrWhiteSpace(userAgent))
        {
            return userAgent.ToString();
        }
        return null;
    }
}