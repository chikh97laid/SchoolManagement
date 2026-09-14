using EduCore.Application.Common.Exceptions;
using FluentValidation;
using System.Text.Json;

namespace EduCore.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse();

        switch (exception)
        {
            case ValidationException validationException:

                context.Response.StatusCode = StatusCodes.Status400BadRequest;

                response.StatusCode = context.Response.StatusCode;
                response.Message = "Validation failed.";

                response.Errors = validationException.Errors
                    .Select(e => !string.IsNullOrWhiteSpace(e.ErrorCode) ? e.ErrorCode : "Common.InvalidFormat")
                    .ToList();

                _logger.LogWarning("Validation failed for request {Path}", context.Request.Path);
                break;

            case BadRequestException:

                context.Response.StatusCode = StatusCodes.Status400BadRequest;

                response.StatusCode = context.Response.StatusCode;
                response.Message = exception.Message;

                _logger.LogWarning("Bad request for request {Path}: {Message}", context.Request.Path, exception.Message);
                break;

            case UnauthorizedException:

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                response.StatusCode = context.Response.StatusCode;
                response.Message = exception.Message;

                _logger.LogWarning("Unauthorized access for request {Path}: {Message}", context.Request.Path, exception.Message);
                break;

            case ForbiddenException:

                context.Response.StatusCode = StatusCodes.Status403Forbidden;

                response.StatusCode = context.Response.StatusCode;
                response.Message = exception.Message;
                
                _logger.LogWarning("Forbidden access for request {Path}: {Message}", context.Request.Path, exception.Message);
                break;

            case NotFoundException:

                context.Response.StatusCode = StatusCodes.Status404NotFound;

                response.StatusCode = context.Response.StatusCode;
                response.Message = exception.Message;
                
                _logger.LogWarning("Not found for request {Path}: {Message}", context.Request.Path, exception.Message);
                break;

            case ConflictException:

                context.Response.StatusCode = StatusCodes.Status409Conflict;
                response.StatusCode = context.Response.StatusCode;
                response.Message = exception.Message;
                
                _logger.LogWarning("Conflict for request {Path}: {Message}", context.Request.Path, exception.Message);
                break;

            default:

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                response.StatusCode = context.Response.StatusCode;
                response.Message = "حدث خطأ غير متوقع.";
                
                _logger.LogError(exception, "Unexpected error occurred for request {Path}: {Message}", context.Request.Path, exception.Message);
                break;
        }

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}