using System.Net;
using System.Text.Json;
using ChatApp.Core.Exceptions;

namespace ChatApp.Api.Middleware;

/// <summary>
/// Global exception handling middleware that catches exceptions and maps them to appropriate HTTP responses.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            NotFoundException notFoundEx => 
                (HttpStatusCode.NotFound, notFoundEx.Message),
            
            ForbiddenException forbiddenEx => 
                (HttpStatusCode.Forbidden, forbiddenEx.Message),
            
            UnauthorizedException unauthorizedEx => 
                (HttpStatusCode.Unauthorized, unauthorizedEx.Message),
            
            BadRequestException badRequestEx => 
                (HttpStatusCode.BadRequest, badRequestEx.Message),
            
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
        };

        // Log the exception
        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning(exception, "Client error occurred: {StatusCode} - {Message}", statusCode, message);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            error = message,
            statusCode = (int)statusCode,
            timestamp = DateTime.UtcNow
        };

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}

