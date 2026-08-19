using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace khaosat_api.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
    }
}
