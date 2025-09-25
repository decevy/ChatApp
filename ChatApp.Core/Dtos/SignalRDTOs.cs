namespace ChatApp.Core.DTOs;

public class TypingIndicatorDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int RoomId { get; set; }
    public bool IsTyping { get; set; }
}
