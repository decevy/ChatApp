using ChatApp.Core.Entities;

namespace ChatApp.Core.Dtos;

public class MessageDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
    public int RoomId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? EditedAt { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? AttachmentFileName { get; set; }
    public MessageType Type { get; set; }
    public List<MessageReactionDto> Reactions { get; set; } = [];

    public static MessageDto FromEntity(Message message) => new()
    {
        Id = message.Id,
        Content = message.Content,
        User = UserDto.FromEntity(message.User),
        RoomId = message.RoomId,
        CreatedAt = message.CreatedAt,
        EditedAt = message.EditedAt,
        AttachmentUrl = message.AttachmentUrl,
        AttachmentFileName = message.AttachmentFileName,
        Type = message.Type
    };
}