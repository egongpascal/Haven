using Haven.Domain.DTO;
using Haven.Domain.Models;

namespace Haven.Application
{
    public interface IGroupService
    {
        Task<Group> CreateGroupAsync(CreateGroupRequest group);
        Task<Group> GetGroupByIdAsync(Guid id);
        Task<Group> UpdateGroupAsync(Guid id, string name);
        Task<Group> GetGroupByInviteCodeAsync(string inviteCode);
        Task<Group> GetGroupWithMembersByInviteCodeAsync(string inviteCode);
        Task<bool> JoinGroupByInviteCodeAsync(JoinGroupRequest request);
        Task<List<GroupMemberWithUserDetails>> GetGroupMembersWithDetailsAsync(Guid groupId);
        Task<Group?> GetUserCurrentGroupAsync(Guid userId);
        Task<List<Group>> GetUserGroupsAsync(Guid userId);
    }
}
