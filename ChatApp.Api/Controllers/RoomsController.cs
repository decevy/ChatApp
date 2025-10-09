using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ChatApp.Core.Dtos;
using ChatApp.Core.Dtos.Requests;
using ChatApp.Core.Dtos.Responses;
using ChatApp.Core.Entities;
using ChatApp.Core.Interfaces;
using System.Security.Claims;
using ChatApp.Core.Extensions;

namespace ChatApp.Api.Controllers;

/// <summary>
/// Manages chat rooms and room memberships
/// </summary>
[Authorize]
[ApiController]
[Route("api/rooms")]
public class RoomsController(
    IRoomRepository roomRepository,
    IUserRepository userRepository,
    ILogger<RoomsController> logger) : ControllerBase
{
    /// <summary>
    /// Get all rooms the current user is a member of
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<RoomDto>), 200)]
    public async Task<IActionResult> GetUserRooms()
    {
        var userId = GetCurrentUserId();
        var rooms = await roomRepository.GetUserRoomsAsync(userId,
            includeCreator: true,
            includeMembers: true);
        
        var roomDtos = new List<RoomDto>();
        foreach (var room in rooms)
        {
            var roomDto = RoomDto.FromEntity(room);
            roomDto.LastMessage = (await roomRepository.GetLastMessageInRoomAsync(room.Id))?
                .Transform(MessageDto.FromEntity);
            roomDtos.Add(roomDto);
        }
        
        return Ok(roomDtos);
    }

    /// <summary>
    /// Get detailed information about a specific room
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RoomDetailsDto), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetRoom(int id)
    {
        var userId = GetCurrentUserId();
        
        // Check if user is a member
        if (!await roomRepository.IsUserMemberAsync(id, userId))
            return Forbid();
        
        var room = await roomRepository.GetByIdWithMembersAsync(id);
        if (room == null)
            return NotFound(new ErrorResponse("Room not found"));

        var response = new RoomDetailsDto
        {
            Id = room.Id,
            Name = room.Name,
            Description = room.Description,
            IsPrivate = room.IsPrivate,
            CreatedAt = room.CreatedAt,
            CreatedById = room.CreatedBy,
            CreatedByUsername = room.Creator.Username,
            Members = room.Members.Select(m => new RoomMemberDto
            {
                UserId = m.UserId,
                Username = m.User.Username,
                Email = m.User.Email,
                Role = m.Role.ToString(),
                JoinedAt = m.JoinedAt
            }).ToList()
        };

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
        var user = await userRepository.GetByIdAsync(userId);
        
        if (user == null)
            return BadRequest(new ErrorResponse("User not found"));

        var room = new Room
        {
            Name = request.Name,
            Description = request.Description,
            IsPrivate = request.IsPrivate,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            Creator = user
        };

        await roomRepository.CreateAsync(room);

        // Add creator as admin member
        await roomRepository.AddRoomMemberAsync(new RoomMember
        {
            UserId = userId,
            RoomId = room.Id,
            Role = RoomRole.Admin,
            JoinedAt = DateTime.UtcNow
        });

        var roomDto = new RoomDto
        {
            Id = room.Id,
            Name = room.Name,
            Description = room.Description,
            IsPrivate = room.IsPrivate,
            CreatedAt = room.CreatedAt,
            Creator = UserDto.FromEntity(user),
            MemberCount = 1,
            LastMessage = null
        };

        return CreatedAtAction(nameof(GetRoom), new { id = room.Id }, roomDto);
    }

    /// <summary>
    /// Update room details (name, description)
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(RoomDto), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateRoom(int id, [FromBody] UpdateRoomRequest request)
    {
        var userId = GetCurrentUserId();
        
        // Check if user is admin
        if (!await roomRepository.IsUserRoomAdminAsync(id, userId))
            return Forbid();

        var room = await roomRepository.GetByIdAsync(id);
        if (room == null)
            return NotFound(new ErrorResponse("Room not found"));

        room.Name = request.Name;
        room.Description = request.Description;
        
        await roomRepository.UpdateAsync(room);

        var roomDto = new RoomDto
        {
            Id = room.Id,
            Name = room.Name,
            Description = room.Description,
            IsPrivate = room.IsPrivate,
            CreatedAt = room.CreatedAt,
            Creator = UserDto.FromEntity(room.Creator),
            MemberCount = room.Members.Count
        };

        return Ok(roomDto);
    }

    /// <summary>
    /// Delete a room (admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        var userId = GetCurrentUserId();
        
        // Check if user is admin
        if (!await roomRepository.IsUserRoomAdminAsync(id, userId))
            return Forbid();

        var room = await roomRepository.GetByIdAsync(id);
        if (room == null)
            return NotFound(new ErrorResponse("Room not found"));

        await roomRepository.DeleteAsync(id);
        
        return NoContent();
    }

    /// <summary>
    /// Add a member to the room
    /// </summary>
    [HttpPost("{id}/members")]
    [ProducesResponseType(typeof(MessageResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AddMember(int id, [FromBody] AddRoomMemberRequest request)
    {
        var userId = GetCurrentUserId();
        
        // Check if user is admin
        if (!await roomRepository.IsUserRoomAdminAsync(id, userId))
            return Forbid();

        var room = await roomRepository.GetByIdAsync(id);
        if (room == null)
            return NotFound(new ErrorResponse("Room not found"));

        // Check if user to add exists
        var userToAdd = await userRepository.GetByIdAsync(request.UserId);
        if (userToAdd == null)
            return BadRequest(new ErrorResponse("User not found"));

        // Check if already a member
        if (await roomRepository.IsUserMemberAsync(id, request.UserId))
            return BadRequest(new ErrorResponse("User is already a member"));

        await roomRepository.AddRoomMemberAsync(new RoomMember
        {
            UserId = request.UserId,
            RoomId = id,
            Role = request.IsAdmin ? RoomRole.Admin : RoomRole.Member,
            JoinedAt = DateTime.UtcNow
        });

        return CreatedAtAction(
            nameof(GetRoom), 
            new { id }, 
            new MessageResponse($"User {userToAdd.Username} added to room"));
    }

    /// <summary>
    /// Remove a member from the room
    /// </summary>
    [HttpDelete("{id}/members/{userId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RemoveMember(int id, int userId)
    {
        var currentUserId = GetCurrentUserId();
        
        // Users can remove themselves, or admins can remove others
        var isAdmin = await roomRepository.IsUserRoomAdminAsync(id, currentUserId);
        if (userId != currentUserId && !isAdmin)
            return Forbid();

        var room = await roomRepository.GetByIdAsync(id);
        if (room == null)
            return NotFound(new ErrorResponse("Room not found"));

        var member = await roomRepository.GetRoomMemberAsync(id, userId);
        if (member == null)
            return NotFound(new ErrorResponse("User is not a member of this room"));

        // Prevent removing the last admin
        if (member.Role == RoomRole.Admin)
        {
            var roomWithMembers = await roomRepository.GetByIdWithMembersAsync(id);
            var adminCount = roomWithMembers?.Members.Count(m => m.Role == RoomRole.Admin) ?? 0;
            if (adminCount == 1)
                return BadRequest(new ErrorResponse("Cannot remove the last admin"));
        }

        await roomRepository.RemoveRoomMemberAsync(id, userId);
        
        return NoContent();
    }

    /// <summary>
    /// Get paginated message history for a room
    /// </summary>
    [HttpGet("{id}/messages")]
    [ProducesResponseType(typeof(PaginatedResponse<MessageDto>), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetRoomMessages(
        int id, 
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 50)
    {
        var userId = GetCurrentUserId();
        
        // Check if user is a member
        if (!await roomRepository.IsUserMemberAsync(id, userId))
            return Forbid();

        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 50;

        var (messages, totalCount) = await roomRepository.GetRoomMessagesPagedAsync(id, page, pageSize);

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