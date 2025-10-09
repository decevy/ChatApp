using ChatApp.Core.Entities;
using ChatApp.Core.Extensions;

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

    public static RoomDto FromEntity(Room room) => new RoomDto
    {
        Id = room.Id,
        Name = room.Name,
        Description = room.Description,
        IsPrivate = room.IsPrivate,
        CreatedAt = room.CreatedAt,
        Creator = ((User?)room.Creator)?.Transform(UserDto.FromEntity)!,
        MemberCount = room.Members.Count,
        LastMessage = room.Messages
            .OrderByDescending(m => m.CreatedAt).FirstOrDefault()?
            .Transform(MessageDto.FromEntity)
    };
}