namespace ChatApp.Core.Dtos;

public class RoomDetailsDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPrivate { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CreatedById { get; set; }
    public string CreatedByUsername { get; set; } = string.Empty;
    public List<RoomMemberDto> Members { get; set; } = [];
}