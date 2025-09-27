namespace ChatApp.Core.Dtos;

public class RoomDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPrivate { get; set; }
    public UserDto Creator { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public int MemberCount { get; set; }
    public MessageDto? LastMessage { get; set; }
}