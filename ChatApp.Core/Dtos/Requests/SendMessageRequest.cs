using System.ComponentModel.DataAnnotations;
using ChatApp.Core.Entities;

namespace ChatApp.Core.Dtos.Requests;

public class SendMessageRequest
{
    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    public int RoomId { get; set; }

    public MessageType Type { get; set; } = MessageType.Text;
}