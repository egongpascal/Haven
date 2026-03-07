using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Haven.API.Hubs
{
    public class GroupHub : Hub
    {
        // Group-scoped events are sent from controllers via IHubContext<GroupHub>.
        // Clients invoke JoinGroup/LeaveGroup to subscribe to group-scoped events.

        public async Task JoinGroup(string groupId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
        }

        public async Task LeaveGroup(string groupId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
        }
    }
}
