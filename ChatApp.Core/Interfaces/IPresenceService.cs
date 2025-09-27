using ChatApp.Core.Dtos;

namespace ChatApp.Core.Interfaces;

public interface IPresenceService
{
    Task SetUserOnlineAsync(int userId, string connectionId);
    Task SetUserOfflineAsync(string connectionId);
    Task<IEnumerable<UserPresenceDto>> GetRoomPresenceAsync(int roomId);
    Task SetUserTypingAsync(int userId, int roomId, bool isTyping);
    Task<UserPresenceDto?> GetUserPresenceAsync(int userId);
}
