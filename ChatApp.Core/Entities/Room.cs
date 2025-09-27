using System.ComponentModel.DataAnnotations;

namespace ChatApp.Core.Entities;

public class Room
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public bool IsPrivate { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public required User Creator { get; set; }
    public ICollection<Message> Messages { get; set; } = [];
    public ICollection<RoomMember> Members { get; set; } = [];
}