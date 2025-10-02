using ChatApp.Core.Entities;

namespace ChatApp.Core.Dtos;

public class UserDto
{
    public static UserDto None => new() { Id = -1 };

    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsOnline { get; set; }
    public DateTime LastSeen { get; set; }

    public static UserDto FromEntity(User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Email = user.Email,
        IsOnline = user.IsOnline,
        LastSeen = user.LastSeen
    };
}