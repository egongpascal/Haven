using Haven.Domain.DTO;
using Haven.Domain.Models;
using Haven.Infrastructure;

namespace Haven.Application
{
    public class GroupService : IGroupService
    {
        private readonly IGroupRepository _groupRepository;
        private readonly IUserRepository _userRepository;

        public GroupService(IGroupRepository groupRepository, IUserRepository userRepository)
        {
            _groupRepository = groupRepository;
            _userRepository = userRepository;
        }

        public async Task<Group> CreateGroupAsync(CreateGroupRequest request)
        {
            var inviteCode = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            var group = new Group
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                CreatedBy = request.CreatedBy,
                CreatedAt = DateTime.UtcNow,
                GeofenceRadius = request.GeofenceRadius,
                IsActive = true,
                InviteCode = inviteCode
            };
            group = await _groupRepository.CreateAsync(group);
            await _groupRepository.AddMemberAsync(group.Id, request.CreatedBy, "Owner");
            return group;
        }

        public async Task<Group> GetGroupByIdAsync(Guid id)
        {
            return await _groupRepository.GetByIdAsync(id);
        }
        public async Task<Group> GetGroupByInviteCodeAsync(string inviteCode)
        {
            return await _groupRepository.GetByInviteCodeAsync(inviteCode);
        }
        public async Task<Group> UpdateGroupAsync(Guid id, string name)
        {
            var group = await _groupRepository.GetByIdAsync(id);
            if (group != null)
            {
                group.Name = name;
                await _groupRepository.UpdateAsync(group);
            }
            return group;
        }

        public async Task<bool> JoinGroupByInviteCodeAsync(JoinGroupRequest request)
        {
            var join = await _groupRepository.JoinGroupByInviteCodeAsync(request);
            return join;
        }   
        public async Task<List<GroupMemberWithUserDetails>> GetGroupMembersWithDetailsAsync(Guid groupId)
        {
            return await _groupRepository.GetGroupMembersWithDetailsAsync(groupId);
        }

        public async Task<Group> GetGroupWithMembersByInviteCodeAsync(string inviteCode)
        {
            // For now, this just returns the group by invite code
            // You can enhance this later to include member details if needed
            return await _groupRepository.GetByInviteCodeAsync(inviteCode);
        }

        public async Task<Group?> GetUserCurrentGroupAsync(Guid userId)
        {
            return await _groupRepository.GetUserActiveGroupAsync(userId);
        }

        public async Task<List<Group>> GetUserGroupsAsync(Guid userId)
        {
            return await _groupRepository.GetUserGroupsAsync(userId);
        }
    }
}
