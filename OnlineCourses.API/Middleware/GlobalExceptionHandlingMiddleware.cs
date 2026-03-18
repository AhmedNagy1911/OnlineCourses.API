using System.Net;

namespace OnlineCourses.API.Middleware;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
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
        catch (Exception exception)
        {
            _logger.LogError(exception, "? Unhandled Exception: {Message} | Path: {Path} | Method: {Method}",
                exception.Message,
                context.Request.Path,
                context.Request.Method);

            await HandleExceptionAsync(context, exception);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        int statusCode = GetStatusCode(exception);
        context.Response.StatusCode = statusCode;

        var response = new
        {
            message = GetMessage(exception),
            statusCode = statusCode,
            timestamp = DateTime.UtcNow,
            errors = new { detail = exception.Message }
        };

        return context.Response.WriteAsJsonAsync(response);
    }

    private static int GetStatusCode(Exception exception) =>
        exception switch
        {
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            ArgumentException or ArgumentNullException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            InvalidOperationException => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.InternalServerError
        };

    private static string GetMessage(Exception exception) =>
        exception switch
        {
            KeyNotFoundException => "Resource not found",
            ArgumentException or ArgumentNullException => "Invalid argument provided",
            UnauthorizedAccessException => "Unauthorized access",
            InvalidOperationException => "Invalid operation",
            _ => "Internal server error"
        };
}