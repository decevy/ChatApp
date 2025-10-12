using ChatApp.Core.Entities;
using ChatApp.Core.QueryBuilders;

namespace ChatApp.Core.Interfaces;

public interface IMessageRepository
{
    MessageQueryBuilder Query();
    Task<Message?> GetByIdAsync(int id);
    Task<Message> CreateAsync(Message message);
    Task UpdateAsync(Message message);
    Task DeleteAsync(int id);
    
    Task<Message?> GetLastMessageInRoomAsync(int roomId);
}
