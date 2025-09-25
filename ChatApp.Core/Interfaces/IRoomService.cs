using ChatApp.Core.DTOs;

namespace ChatApp.Core.Interfaces;

public interface IRoomService
{
    Task<RoomDto?> GetRoomAsync(int id);
    Task<RoomDetailDto?> GetRoomWithDetailsAsync(int id);
    Task<IEnumerable<RoomDto>> GetUserRoomsAsync(int userId);
    Task<IEnumerable<RoomDto>> GetPublicRoomsAsync();
    Task<RoomDto> CreateRoomAsync(CreateRoomRequest request, int creatorId);
    Task JoinRoomAsync(int roomId, int userId);
    Task LeaveRoomAsync(int roomId, int userId);
    Task<bool> IsUserMemberAsync(int roomId, int userId);
}
