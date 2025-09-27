using System.ComponentModel.DataAnnotations;

namespace ChatApp.Core.Entities;

public class Message
{
    public static Message None => new() { Id = -1 };

    public int Id { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;
    
    public int UserId { get; set; }
    public int RoomId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EditedAt { get; set; }
    
    public string? AttachmentUrl { get; set; }
    public string? AttachmentFileName { get; set; }
    public MessageType Type { get; set; } = MessageType.Text;

    // Navigation properties
    public User User { get; set; } = User.None;
    public Room Room { get; set; } = Room.None;
    public ICollection<MessageReaction> Reactions { get; set; } = [];
}