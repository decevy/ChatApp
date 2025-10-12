using System.ComponentModel.DataAnnotations;

namespace ChatApp.Core.Dtos.Requests;

public class UpdateUserRequest
{
    [StringLength(50, MinimumLength = 3)]
    public string? Username { get; set; }

    [EmailAddress]
    public string? Email { get; set; }
}

