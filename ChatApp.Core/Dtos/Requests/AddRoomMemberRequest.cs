using System.ComponentModel.DataAnnotations;

namespace ChatApp.Core.Dtos.Requests;

public class AddRoomMemberRequest
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Invalid user ID")]
    public int UserId { get; set; }
    
    public bool IsAdmin { get; set; } = false;
}