namespace ChatApp.Core.Entities;

public class RoomMember
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int RoomId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public RoomRole Role { get; set; } = RoomRole.Member;
    
    // Navigation properties
    public required User User { get; set; }
    public required Room Room { get; set; }
}
