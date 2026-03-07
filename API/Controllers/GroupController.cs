using Haven.API.Hubs;
using Haven.Application;
using Haven.Domain.DTO;
using Haven.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Haven.API.Controllers
{
    [ApiController]
    [Route("api/groups")]
    [Authorize]
    public class GroupController : ControllerBase
    {
        private readonly IGroupService _groupService;
        private readonly IHubContext<GroupHub> _groupHubContext;
        private readonly IHubContext<NotificationHub> _notificationHubContext;
        private readonly IGroupRepository _groupRepository;
        private readonly IUserRepository _userRepository;

        public GroupController(
            IGroupService groupService,
            IHubContext<GroupHub> groupHubContext,
            IHubContext<NotificationHub> notificationHubContext,
            IGroupRepository groupRepository,
            IUserRepository userRepository)
        {
            _groupService = groupService;
            _groupHubContext = groupHubContext;
            _notificationHubContext = notificationHubContext;
            _groupRepository = groupRepository;
            _userRepository = userRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateGroupRequest request)
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();
            request.CreatedBy = userId;
            var group = await _groupService.CreateGroupAsync(request);
            return Ok(group);
        }

        [HttpGet("by-invite/{inviteCode}")]
        public async Task<IActionResult> GetByInviteCode(string inviteCode)
        {
            var group = await _groupService.GetGroupByInviteCodeAsync(inviteCode);
            if (group == null) return NotFound();
            return Ok(group);
        }

        [HttpGet("by-invite/{inviteCode}/with-members")]
        public async Task<IActionResult> GetByInviteCodeWithMembers(string inviteCode)
        {
            var group = await _groupService.GetGroupWithMembersByInviteCodeAsync(inviteCode);
            if (group == null) return NotFound();
            return Ok(group);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var group = await _groupService.GetGroupByIdAsync(id);
            if (group == null) return NotFound();
            return Ok(group);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGroupRequest request)
        {
            var group = await _groupService.UpdateGroupAsync(id, request.Name);
            if (group == null) return NotFound();

            // Notify group members that the group metadata changed
            await _groupHubContext.Clients.Group(id.ToString())
                .SendAsync("GroupUpdated", new { groupId = id.ToString() });

            return Ok(group);
        }

        [HttpPost("join")]
        public async Task<IActionResult> JoinGroup([FromBody] JoinGroupRequest request)
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();
            request.UserId = userId;
            var result = await _groupService.JoinGroupByInviteCodeAsync(request);
            if (!result)
                return BadRequest("Could not join group. Invite code may be invalid or user already a member.");

            // Broadcast MemberJoined + NotificationReceived to the group
            try
            {
                var group = await _groupService.GetGroupByInviteCodeAsync(request.InviteCode);
                var user = await _userRepository.GetByIdAsync(userId);
                if (group != null && user != null)
                {
                    var displayName = $"{user.FirstName} {user.LastName}".Trim();
                    if (string.IsNullOrEmpty(displayName)) displayName = user.Username;
                    var groupIdStr = group.Id.ToString();

                    await _groupHubContext.Clients.Group(groupIdStr)
                        .SendAsync("MemberJoined", new
                        {
                            groupId = groupIdStr,
                            userId = userId.ToString(),
                            displayName,
                            role = "Member"
                        });

                    await _notificationHubContext.Clients.Group(groupIdStr)
                        .SendAsync("NotificationReceived", new
                        {
                            id = Guid.NewGuid().ToString(),
                            type = "MemberJoined",
                            title = "New member joined",
                            body = $"{displayName} has joined {group.Name}",
                            data = new { groupId = groupIdStr, userId = userId.ToString() },
                            channel = "InApp",
                            sentAt = DateTime.UtcNow.ToString("o"),
                            readAt = (string?)null,
                            isRead = false
                        });
                }
            }
            catch { /* non-critical */ }

            return Ok(new { Message = "Joined group successfully." });
        }

        [HttpPost("{id}/leave")]
        public async Task<IActionResult> LeaveGroup(Guid id)
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            await _groupRepository.RemoveMemberAsync(id, userId);

            string groupIdStr = id.ToString();
            string userIdStr = userId.ToString();

            await _groupHubContext.Clients.Group(groupIdStr)
                .SendAsync("MemberLeft", new { groupId = groupIdStr, userId = userIdStr });

            // Notify remaining members via NotificationHub
            try
            {
                var leavingUser = await _userRepository.GetByIdAsync(userId);
                var group = await _groupRepository.GetByIdAsync(id);
                if (leavingUser != null && group != null)
                {
                    var displayName = $"{leavingUser.FirstName} {leavingUser.LastName}".Trim();
                    if (string.IsNullOrEmpty(displayName)) displayName = leavingUser.Username;

                    await _notificationHubContext.Clients.Group(groupIdStr)
                        .SendAsync("NotificationReceived", new
                        {
                            id = Guid.NewGuid().ToString(),
                            type = "MemberJoined",
                            title = "Member left",
                            body = $"{displayName} has left {group.Name}",
                            data = new { groupId = groupIdStr, userId = userIdStr },
                            channel = "InApp",
                            sentAt = DateTime.UtcNow.ToString("o"),
                            readAt = (string?)null,
                            isRead = false
                        });
                }
            }
            catch { /* non-critical */ }

            return Ok(new { Message = "Left group successfully." });
        }

        [HttpGet("{id}/members")]
        public async Task<IActionResult> GetGroupMembersWithDetailsAsync(Guid id)
        {
            var members = await _groupService.GetGroupMembersWithDetailsAsync(id);
            if (members == null) return NotFound();
            return Ok(members);
        }

        [HttpGet("user/{userId}/current")]
        public async Task<IActionResult> GetUserCurrentGroup(Guid userId)
        {
            var group = await _groupService.GetUserCurrentGroupAsync(userId);
            if (group == null) return NotFound("User is not in any group");
            return Ok(group);
        }

        [HttpGet("user/{userId}/all")]
        public async Task<IActionResult> GetUserGroups(Guid userId)
        {
            var groups = await _groupService.GetUserGroupsAsync(userId);
            return Ok(groups);
        }
    }
}
