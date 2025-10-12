using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ChatApp.Core.Dtos;
using ChatApp.Core.Dtos.Requests;
using ChatApp.Core.Dtos.Responses;
using ChatApp.Core.Interfaces;
using System.Security.Claims;

namespace ChatApp.Api.Controllers;

/// <summary>
/// Manages chat rooms and room memberships
/// </summary>
[Authorize]
[ApiController]
[Route("api/rooms")]
public class RoomsController(IRoomService roomService) : ControllerBase
{
    /// <summary>
    /// Get all rooms the current user is a member of
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<RoomSummaryDto>), 200)]
    public async Task<IActionResult> GetUserRooms()
    {
        var userId = GetCurrentUserId();
        var rooms = await roomService.GetUserRoomsAsync(userId);
        return Ok(rooms);
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
        
        var room = await roomService.GetRoomAsync(roomId, userId);
        if (room == null)
            return NotFound(new ErrorResponse("Room not found"));

        return Ok(room);
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
        
        var room = await roomService.CreateRoomAsync(request, userId);
        return CreatedAtAction(nameof(GetRoom), new { roomId = room.Id }, room);
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
        
        var room = await roomService.UpdateRoomAsync(roomId, request, userId);
        return Ok(room);
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

        await roomService.DeleteRoomAsync(roomId, userId);
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
        
        await roomService.AddMemberAsync(roomId, request, userId);
        return CreatedAtAction(
            nameof(GetRoom), 
            new { roomId }, 
            new MessageResponse("Member added successfully"));
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
        
        await roomService.RemoveMemberAsync(roomId, userId, currentUserId);
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
        
        var response = await roomService.GetRoomMessagesAsync(roomId, userId, page, pageSize);
        return Ok(response);
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}