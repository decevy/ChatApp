using System.ComponentModel.DataAnnotations;

namespace ChatApp.Core.Dtos.Requests;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}