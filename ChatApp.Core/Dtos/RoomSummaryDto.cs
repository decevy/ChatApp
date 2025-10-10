using ChatApp.Core.Entities;
using ChatApp.Core.Extensions;

namespace ChatApp.Core.Dtos;

public class RoomSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPrivate { get; set; }
    public UserDto Creator { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public int MemberCount { get; set; }
    public MessageDto? LastMessage { get; set; }

    public static RoomSummaryDto FromEntity(Room room) => new RoomSummaryDto
    {
        Id = room.Id,
        Name = room.Name,
        Description = room.Description,
        IsPrivate = room.IsPrivate,
        Creator = UserDto.FromEntity(room.Creator),
        CreatedAt = room.CreatedAt,
        MemberCount = room.Members.Count,
        LastMessage = room.Messages
            .OrderByDescending(m => m.CreatedAt).FirstOrDefault()?
            .Transform(MessageDto.FromEntity)
    };
}