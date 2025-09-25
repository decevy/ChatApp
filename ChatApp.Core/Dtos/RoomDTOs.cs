using System.ComponentModel.DataAnnotations;

namespace ChatApp.Core.DTOs;

public class CreateRoomRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public bool IsPrivate { get; set; }
}

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

public class RoomDetailDto : RoomDto
{
    public List<UserDto> Members { get; set; } = new();
    public List<MessageDto> RecentMessages { get; set; } = new();
}
