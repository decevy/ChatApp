namespace ChatApp.Core.Dtos;

public class MessageReactionDto
{
    public string Emoji { get; set; } = string.Empty;
    public List<UserDto> Users { get; set; } = new();
    public int Count { get; set; }
}