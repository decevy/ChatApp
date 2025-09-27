namespace ChatApp.Core.Dtos;

public class RoomDetailDto : RoomDto
{
    public List<UserDto> Members { get; set; } = new();
    public List<MessageDto> RecentMessages { get; set; } = new();
}