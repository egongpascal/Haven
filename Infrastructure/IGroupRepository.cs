using Haven.Domain.DTO;
using Haven.Domain.Models;

namespace Haven.Infrastructure
{
    public interface IGroupRepository
    {
        Task<Group> CreateAsync(Group group);
        Task<Group> GetByIdAsync(Guid id);
        Task<Group> GetByInviteCodeAsync(string inviteCode);
        Task<Group> UpdateAsync(Group group);
        Task<bool> JoinGroupByInviteCodeAsync(JoinGroupRequest request);
        Task<bool> AddMemberAsync(Guid groupId, Guid userId, string role);
        Task<List<GroupMemberWithUserDetails>> GetGroupMembersWithDetailsAsync(Guid groupId);
        Task<IEnumerable<Group>> GetAllAsync();
        Task<Group?> GetUserActiveGroupAsync(Guid userId);
        Task<List<Group>> GetUserGroupsAsync(Guid userId);
    }
}
