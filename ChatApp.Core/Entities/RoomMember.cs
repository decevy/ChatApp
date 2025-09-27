namespace ChatApp.Core.Entities;

public class RoomMember
{
    public static RoomMember None => new() { Id = -1 };

    public int Id { get; set; }
    public int UserId { get; set; }
    public int RoomId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public RoomRole Role { get; set; } = RoomRole.Member;

    // Navigation properties
    public User User { get; set; } = User.None;
    public Room Room { get; set; } = Room.None;
}
