using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Haven.API.Hubs
{
    public class NotificationHub : Hub
    {
        // Clients join their group room so the server can broadcast group-scoped notifications.
        public async Task JoinGroup(string groupId) =>
            await Groups.AddToGroupAsync(Context.ConnectionId, groupId);

        public async Task LeaveGroup(string groupId) =>
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
    }
}
