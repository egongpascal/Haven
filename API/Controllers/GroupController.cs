using Haven.Application;
using Haven.Domain.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Haven.API.Controllers
{
    [ApiController]
    [Route("api/groups")]
    [Authorize]
    public class GroupController : ControllerBase

    {
        private readonly IGroupService _groupService;

        public GroupController(IGroupService groupService)
        {
            _groupService = groupService;
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
            return Ok(new { Message = "Joined group successfully." });
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
