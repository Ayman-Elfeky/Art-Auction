using ArtAuction.Application.Common.Exceptions;
using System.Net;
using System.Text.Json;

namespace ArtAuction.API.Middleware;

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
            _logger.LogError(ex, "An unhandled exception has occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new { error = exception.Message };

        return exception switch
        {
            NotFoundException => HandleNotFoundException(context, (NotFoundException)exception),
            ForbiddenException => HandleForbiddenException(context, (ForbiddenException)exception),
            ValidationException => HandleValidationException(context, (ValidationException)exception),
            _ => HandleGenericException(context, exception)
        };
    }

    private static Task HandleNotFoundException(HttpContext context, NotFoundException exception)
    {
        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        return context.Response.WriteAsJsonAsync(new { error = exception.Message });
    }

    private static Task HandleForbiddenException(HttpContext context, ForbiddenException exception)
    {
        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
        return context.Response.WriteAsJsonAsync(new { error = exception.Message });
    }

    private static Task HandleValidationException(HttpContext context, ValidationException exception)
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        return context.Response.WriteAsJsonAsync(new { error = exception.Message });
    }

    private static Task HandleGenericException(HttpContext context, Exception exception)
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        return context.Response.WriteAsJsonAsync(new { error = "An internal server error occurred" });
    }
}
