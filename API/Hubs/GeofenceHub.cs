using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Haven.API.Hubs
{
    public class GeofenceHub : Hub
    {
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
