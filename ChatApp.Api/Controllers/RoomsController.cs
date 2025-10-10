using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ChatApp.Core.Dtos;
using ChatApp.Core.Dtos.Requests;
using ChatApp.Core.Dtos.Responses;
using ChatApp.Core.Entities;
using ChatApp.Core.Interfaces;
using System.Security.Claims;

namespace ChatApp.Api.Controllers;

/// <summary>
/// Manages chat rooms and room memberships
/// </summary>
[Authorize]
[ApiController]
[Route("api/rooms")]
public class RoomsController(
    IRoomRepository roomRepository,
    IMessageRepository messageRepository,
    IUserRepository userRepository,
    ILogger<RoomsController> logger) : ControllerBase
{
    /// <summary>
    /// Get all rooms the current user is a member of
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<RoomSummaryDto>), 200)]
    public async Task<IActionResult> GetUserRooms()
    {
        var userId = GetCurrentUserId();
        var rooms = await roomRepository.Query()
            .WithCreator()
            .WithMembers()
            .WhereUserIsMember(userId)
            .ToListAsync();
        
        var roomDtos = new List<RoomSummaryDto>();
        foreach (var room in rooms)
        {
            var lastMessage = await messageRepository.GetLastMessageInRoomAsync(room.Id);
            if (lastMessage != null)
                room.Messages.Add(lastMessage);
            var roomDto = RoomSummaryDto.FromEntity(room);
            roomDtos.Add(roomDto);
        }
        
        return Ok(roomDtos);
    }

    /// <summary>
    /// Get detailed information about a specific room
    /// </summary>
    [HttpGet("{roomId}")]
    [ProducesResponseType(typeof(RoomDto), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetRoom(int roomId)
    {
        var userId = GetCurrentUserId();
        
        // Check if user is a member
        if (!await roomRepository.IsUserMemberAsync(roomId, userId))
            return Forbid();
        
        var room = await roomRepository.Query()
            .WithCreator()
            .WithMembers()
            .FindByIdAsync(roomId);
        if (room == null)
            return NotFound(new ErrorResponse("Room not found"));

        var response = RoomDto.FromEntity(room);
        return Ok(response);
    }

    /// <summary>
    /// Create a new room
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RoomDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest request)
    {
        var userId = GetCurrentUserId();
        var user = await userRepository.FindByIdAsync(userId);
        if (user == null)
            return BadRequest(new ErrorResponse("User not found"));

        // Create room
        var room = new Room
        {
            Name = request.Name,
            Description = request.Description,
            IsPrivate = request.IsPrivate,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            Creator = user
        };
        room = await roomRepository.CreateAsync(room);

        // Add creator as admin member
        var roomMember = new RoomMember
        {
            UserId = userId,
            RoomId = room.Id,
            Role = RoomRole.Admin,
            JoinedAt = DateTime.UtcNow
        };
        await roomRepository.AddRoomMemberAsync(roomMember);
        room.Members.Add(roomMember);

        var roomDto = RoomDto.FromEntity(room);
        return CreatedAtAction(nameof(GetRoom), new { roomId = room.Id }, roomDto);
    }

    /// <summary>
    /// Update room details (name, description)
    /// </summary>
    [HttpPut("{roomId}")]
    [ProducesResponseType(typeof(RoomDto), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateRoom(int roomId, [FromBody] UpdateRoomRequest request)
    {
        var userId = GetCurrentUserId();
        
        // Check if user is admin
        if (!await roomRepository.IsUserRoomAdminAsync(roomId, userId))
            return Forbid();

        var room = await roomRepository.FindByIdAsync(roomId);
        if (room == null)
            return NotFound(new ErrorResponse("Room not found"));

        room.Name = request.Name;
        room.Description = request.Description;
        
        await roomRepository.UpdateAsync(room);

        room = await roomRepository.Query()
            .WithCreator()
            .WithMembers()
            .GetByIdAsync(room.Id);
        var roomDto = RoomDto.FromEntity(room);

        return Ok(roomDto);
    }

    /// <summary>
    /// Delete a room (admin only)
    /// </summary>
    [HttpDelete("{roomId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteRoom(int roomId)
    {
        var userId = GetCurrentUserId();
        
        // Check if user is admin
        if (!await roomRepository.IsUserRoomAdminAsync(roomId, userId))
            return Forbid();

        var room = await roomRepository.FindByIdAsync(roomId);
        if (room == null)
            return NotFound(new ErrorResponse("Room not found"));

        await roomRepository.DeleteAsync(roomId);
        
        return NoContent();
    }

    /// <summary>
    /// Add a member to the room
    /// </summary>
    [HttpPost("{roomId}/members")]
    [ProducesResponseType(typeof(MessageResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AddMember(int roomId, [FromBody] AddRoomMemberRequest request)
    {
        var userId = GetCurrentUserId();
        
        // Check if user is admin
        if (!await roomRepository.IsUserRoomAdminAsync(roomId, userId))
            return Forbid();

        var room = await roomRepository.FindByIdAsync(roomId);
        if (room == null)
            return NotFound(new ErrorResponse("Room not found"));

        // Check if user to add exists
        var userToAdd = await userRepository.FindByIdAsync(request.UserId);
        if (userToAdd == null)
            return BadRequest(new ErrorResponse("User not found"));

        // Check if already a member
        if (await roomRepository.IsUserMemberAsync(roomId, request.UserId))
            return BadRequest(new ErrorResponse("User is already a member"));

        await roomRepository.AddRoomMemberAsync(new RoomMember
        {
            UserId = request.UserId,
            RoomId = roomId,
            Role = request.IsAdmin ? RoomRole.Admin : RoomRole.Member,
            JoinedAt = DateTime.UtcNow
        });

        return CreatedAtAction(
            nameof(GetRoom), 
            new { roomId }, 
            new MessageResponse($"User {userToAdd.Username} added to room"));
    }

    /// <summary>
    /// Remove a member from the room
    /// </summary>
    [HttpDelete("{roomId}/members/{userId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RemoveMember(int roomId, int userId)
    {
        var currentUserId = GetCurrentUserId();
        
        // Users can remove themselves, or admins can remove others
        var isRemovingOther = userId != currentUserId;
        var isAdmin = await roomRepository.IsUserRoomAdminAsync(roomId, currentUserId);
        if (isRemovingOther && !isAdmin)
            return Forbid();

        var room = await roomRepository.FindByIdAsync(roomId);
        if (room == null)
            return NotFound(new ErrorResponse("Room not found"));

        var member = await roomRepository.GetRoomMemberAsync(roomId, userId);
        if (member == null)
            return NotFound(new ErrorResponse("User is not a member of this room"));

        // Prevent removing the last admin
        if (member.Role == RoomRole.Admin)
        {
            var roomWithMembers = await roomRepository.Query().WithMembers().FindByIdAsync(roomId);
            var adminCount = roomWithMembers?.Members.Count(m => m.Role == RoomRole.Admin) ?? 0;
            if (adminCount == 1)
                return BadRequest(new ErrorResponse("Cannot remove the last admin"));
        }

        await roomRepository.RemoveRoomMemberAsync(roomId, userId);
        
        return NoContent();
    }

    /// <summary>
    /// Get paginated message history for a room
    /// </summary>
    [HttpGet("{roomId}/messages")]
    [ProducesResponseType(typeof(PaginatedResponse<MessageDto>), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetRoomMessages(
        int roomId, 
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 50)
    {
        var userId = GetCurrentUserId();
        
        // Check if user is a member
        if (!await roomRepository.IsUserMemberAsync(roomId, userId))
            return Forbid();

        if (page < 1) page = 1;
        if (pageSize is < 1 or > 100) pageSize = 50;

        var (messages, totalCount) = await messageRepository.Query()
            .WithUser()
            .WhereRoomId(roomId)
            .ToPagedListAsync(page, pageSize);

        var response = new PaginatedResponse<MessageDto>
        {
            Items = messages.Select(MessageDto.FromEntity).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return Ok(response);
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}