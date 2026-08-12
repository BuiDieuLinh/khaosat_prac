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
}