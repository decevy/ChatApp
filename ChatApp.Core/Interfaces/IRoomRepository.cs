using ChatApp.Core.Entities;

namespace ChatApp.Core.Interfaces;

public interface IRoomRepository
{
    #region Room operations
    Task<Room> CreateAsync(Room room);
    Task<Room?> GetByIdAsync(int id);
    Task<Room?> GetByIdWithMembersAsync(int id);
    Task<IEnumerable<Room>> GetUserRoomsAsync(int userId,
        bool includeCreator = false,
        bool includeMembers = false, bool includeMemberUsers = false,
        bool includeMessages = false, bool includeMessageUsers = false);
    Task<IEnumerable<Room>> GetPublicRoomsAsync();
    Task UpdateAsync(Room room);
    Task DeleteAsync(int id);
    #endregion

    #region Room Member operations
    Task<RoomMember?> GetRoomMemberAsync(int roomId, int userId);
    Task AddRoomMemberAsync(RoomMember member);
    Task RemoveRoomMemberAsync(int roomId, int userId);
    Task<bool> IsUserMemberAsync(int roomId, int userId);
    Task<bool> IsUserRoomAdminAsync(int roomId, int userId);
    #endregion

    #region Message operations
    Task<Message?> GetLastMessageInRoomAsync(int roomId);
    Task<(IEnumerable<Message> messages, int totalCount)> GetRoomMessagesPagedAsync(int roomId, int page, int pageSize);
    #endregion
}
