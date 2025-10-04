namespace ChatApp.Core.Dtos;

public class RoomEventDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int RoomId { get; set; }
    public DateTime Timestamp { get; set; }
}