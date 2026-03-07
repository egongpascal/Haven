using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Haven.API.Hubs
{
    public class NotificationHub : Hub
    {
        // Notification hub is user-scoped; clients connect individually.
        // Server sends NotificationReceived to specific users via IHubContext<NotificationHub>.
    }
}
