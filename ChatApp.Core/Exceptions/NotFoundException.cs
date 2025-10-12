namespace ChatApp.Core.Exceptions;

/// <summary>
/// Exception thrown when a requested resource is not found.
/// Maps to HTTP 404 Not Found.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
    
    public NotFoundException(string entityName, object key) 
        : base($"{entityName} with ID '{key}' not found")
    {
    }
}

