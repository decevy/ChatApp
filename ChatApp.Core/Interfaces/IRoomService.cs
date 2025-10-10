using ChatApp.Core.Dtos;
using ChatApp.Core.Dtos.Requests;

namespace ChatApp.Core.Interfaces;

public interface IRoomService
{
    Task<RoomSummaryDto?> GetRoomAsync(int id);
    Task<RoomDetailDto?> GetRoomWithDetailsAsync(int id);
    Task<IEnumerable<RoomSummaryDto>> GetUserRoomsAsync(int userId);
    Task<IEnumerable<RoomSummaryDto>> GetPublicRoomsAsync();
    Task<RoomSummaryDto> CreateRoomAsync(CreateRoomRequest request, int creatorId);
    Task JoinRoomAsync(int roomId, int userId);
    Task LeaveRoomAsync(int roomId, int userId);
    Task<bool> IsUserMemberAsync(int roomId, int userId);
}
