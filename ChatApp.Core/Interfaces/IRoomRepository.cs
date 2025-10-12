using ChatApp.Core.Entities;
using ChatApp.Core.QueryBuilders;

namespace ChatApp.Core.Interfaces;

public interface IRoomRepository
{
    RoomQueryBuilder Query();
    Task<Room> GetByIdAsync(int id);
    Task<Room?> FindByIdAsync(int id);
    Task<Room> CreateAsync(Room room);
    Task UpdateAsync(Room room);
    Task DeleteAsync(int id);

    RoomMemberQueryBuilder QueryRoomMembers();
    Task<RoomMember?> FindRoomMemberAsync(int roomId, int userId);
    Task AddRoomMemberAsync(RoomMember member);
    Task RemoveRoomMemberAsync(int roomId, int userId);
    Task<bool> IsUserMemberAsync(int roomId, int userId);
    Task<bool> IsUserRoomAdminAsync(int roomId, int userId);
    Task<bool> ExistsAsync(int id);
}
