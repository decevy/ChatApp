using ChatApp.Core.Entities;

namespace ChatApp.Core.Interfaces;

public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(int id);
    Task<Room?> GetByIdWithMembersAsync(int id);
    Task<IEnumerable<Room>> GetUserRoomsAsync(int userId);
    Task<IEnumerable<Room>> GetPublicRoomsAsync();
    Task<Room> CreateAsync(Room room);
    Task UpdateAsync(Room room);
    Task DeleteAsync(int id);
    Task<bool> IsUserMemberAsync(int roomId, int userId);
}
