using ChatApp.Core.Dtos;
using ChatApp.Core.Dtos.Requests;
using ChatApp.Core.Dtos.Responses;

namespace ChatApp.Core.Interfaces;

public interface IRoomService
{
    // Room operations
    Task<List<RoomSummaryDto>> GetUserRoomsAsync(int userId);
    Task<RoomDto?> GetRoomAsync(int roomId, int userId);
    Task<RoomDto> CreateRoomAsync(CreateRoomRequest request, int userId);
    Task<RoomDto?> UpdateRoomAsync(int roomId, UpdateRoomRequest request, int userId);
    Task<bool> DeleteRoomAsync(int roomId, int userId);
    
    // Member operations
    Task<string> AddMemberAsync(int roomId, AddRoomMemberRequest request, int userId);
    Task<bool> RemoveMemberAsync(int roomId, int userIdToRemove, int currentUserId);
    
    // Message operations
    Task<PaginatedResponse<MessageDto>> GetRoomMessagesAsync(int roomId, int userId, int page, int pageSize);
    
    // Methods not yet implemented
    Task<IEnumerable<RoomSummaryDto>> GetPublicRoomsAsync();
    Task JoinRoomAsync(int roomId, int userId);
    Task LeaveRoomAsync(int roomId, int userId);
}
