using ChatApp.Core.Entities;

namespace ChatApp.Core.Interfaces;

public interface IMessageRepository
{
    Task<Message?> GetByIdAsync(int id);
    Task<IEnumerable<Message>> GetRoomMessagesAsync(int roomId, int page = 1, int pageSize = 50);
    Task<Message> CreateAsync(Message message);
    Task UpdateAsync(Message message);
    Task DeleteAsync(int id);
    Task<Message?> GetLastMessageInRoomAsync(int roomId);
}
