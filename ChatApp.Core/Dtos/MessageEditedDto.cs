namespace ChatApp.Core.Dtos;

public class MessageEditedDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime EditedAt { get; set; }
}