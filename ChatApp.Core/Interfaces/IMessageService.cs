using ChatApp.Core.Dtos;

namespace ChatApp.Core.Interfaces;

public interface IMessageService
{
    Task<MessageDto?> GetMessageAsync(int id);
    Task<IEnumerable<MessageDto>> GetRoomMessagesAsync(int roomId, int page = 1, int pageSize = 50);
    Task<MessageDto> SendMessageAsync(SendMessageRequest request, int userId);
    Task<MessageDto> UpdateMessageAsync(int messageId, string content, int userId);
    Task DeleteMessageAsync(int messageId, int userId);
    Task<MessageDto> AddReactionAsync(int messageId, string emoji, int userId);
    Task RemoveReactionAsync(int messageId, string emoji, int userId);
}
