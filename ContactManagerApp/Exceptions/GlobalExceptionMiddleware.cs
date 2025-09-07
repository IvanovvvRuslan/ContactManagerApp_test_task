namespace ContactManagerApp.Exceptions;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
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

    public async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        int statusCode;
        object response;

        switch (exception)
        {
            case NotFoundException notFoundException:
                statusCode = StatusCodes.Status404NotFound;
                _logger.LogWarning("Not found: {Message}", notFoundException.Message);
                response = new { Status = "Error", Message = notFoundException.Message };
                break;
            
            default:
                statusCode = StatusCodes.Status500InternalServerError;
                _logger.LogError(exception, "Unhandled exception");
                response = new { Status = "Error", Message = "An unexpected error occurred." };
                break;
        }
        
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(response);
    }
}