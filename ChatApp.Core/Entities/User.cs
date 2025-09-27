using System.ComponentModel.DataAnnotations;

namespace ChatApp.Core.Entities;

public class User
{
    public static User None => new() { Id = -1 };

    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    public bool IsOnline { get; set; }

    // Navigation properties
    public ICollection<Message> Messages { get; set; } = [];
    public ICollection<RoomMember> RoomMemberships { get; set; } = [];
}