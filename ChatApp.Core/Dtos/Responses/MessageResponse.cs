using System;

namespace ChatApp.Core.Dtos.Responses;

public class MessageResponse(string message)
{
    public string Message { get; set; } = message;
}
