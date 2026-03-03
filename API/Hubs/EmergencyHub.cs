using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Haven.API.Hubs
{
    public class EmergencyHub : Hub
    {
        public async Task TriggerSOS(string groupId, string userId, double latitude, double longitude, string emergencyType)
        {
            await Clients.Group(groupId).SendAsync("SOSAlert", new {
                UserId = userId,
                Latitude = latitude,
                Longitude = longitude,
                EmergencyType = emergencyType
            });
        }

        public async Task ResolveSOS(string groupId, string emergencyId)
        {
            await Clients.Group(groupId).SendAsync("SOSResolved", emergencyId);
        }

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
