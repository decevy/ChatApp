using ChatApp.Core.Entities;

namespace ChatApp.Core.Dtos;

public class RoomMemberDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }

    public static RoomMemberDto FromEntity(RoomMember member) => new RoomMemberDto
    {
        UserId = member.UserId,
        Username = member.User.Username,
        Email = member.User.Email,
        Role = member.Role.ToString(),
        JoinedAt = member.JoinedAt
    };
}