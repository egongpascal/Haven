using Microsoft.AspNetCore.SignalR;

namespace Haven.API.Hubs
{
    public class GroupHub : Hub
    {
        public async Task NotifyMemberJoined(string groupId, string userId)
        {
            await Clients.Group(groupId).SendAsync("MemberJoined", userId);
        }

        public async Task NotifyGroupUpdated(string groupId)
        {
            await Clients.Group(groupId).SendAsync("GroupUpdated", groupId);
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
