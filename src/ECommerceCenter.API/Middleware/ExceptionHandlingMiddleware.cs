using System.Net;
using System.Text.Json;
using ECommerceCenter.Application.Common.ApiResponse;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ECommerceCenter.API.Middleware;

public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        var (statusCode, response) = exception switch
        {
            ValidationException validationException =>
                HandleValidationException(validationException),
            DbUpdateConcurrencyException =>
                HandleConcurrencyException(),
            DbUpdateException dbEx =>
                HandleDbUpdateException(dbEx),
            UnauthorizedAccessException =>
                HandleUnauthorizedAccessException(),
            KeyNotFoundException keyEx =>
                HandleKeyNotFoundException(keyEx),
            ArgumentException argEx =>
                HandleArgumentException(argEx),
            InvalidOperationException invEx =>
                HandleInvalidOperationException(invEx),
            _ => HandleUnknownException(exception)
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }

    private static (HttpStatusCode, ApiResponse<object>) HandleValidationException(
        ValidationException exception)
    {
        var errors = exception.Errors
            .Select(e => e.ErrorMessage)
            .ToList();

        return (HttpStatusCode.BadRequest, new ApiResponse<object>
        {
            StatusCode = HttpStatusCode.BadRequest,
            Succeeded = false,
            Message = "Validation failed.",
            Errors = errors
        });
    }

    private static (HttpStatusCode, ApiResponse<object>) HandleConcurrencyException()
        => (HttpStatusCode.Conflict, new ApiResponse<object>
        {
            StatusCode = HttpStatusCode.Conflict,
            Succeeded = false,
            Message = "A concurrency conflict occurred. The record was modified by another user."
        });

    private static (HttpStatusCode, ApiResponse<object>) HandleDbUpdateException(
        DbUpdateException exception)
    {
        var inner = exception.InnerException?.Message ?? "";

        if (inner.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) ||
            inner.Contains("duplicate", StringComparison.OrdinalIgnoreCase))
            return (HttpStatusCode.Conflict, new ApiResponse<object>
            {
                StatusCode = HttpStatusCode.Conflict,
                Succeeded = false,
                Message = "A duplicate record with this information already exists."
            });

        if (inner.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase) ||
            inner.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase))
            return (HttpStatusCode.BadRequest, new ApiResponse<object>
            {
                StatusCode = HttpStatusCode.BadRequest,
                Succeeded = false,
                Message = "The operation violates a data integrity constraint."
            });

        return (HttpStatusCode.InternalServerError, new ApiResponse<object>
        {
            StatusCode = HttpStatusCode.InternalServerError,
            Succeeded = false,
            Message = "A database error occurred."
        });
    }

    private static (HttpStatusCode, ApiResponse<object>) HandleUnauthorizedAccessException()
        => (HttpStatusCode.Unauthorized, new ApiResponse<object>
        {
            StatusCode = HttpStatusCode.Unauthorized,
            Succeeded = false,
            Message = "Unauthorized access."
        });

    private static (HttpStatusCode, ApiResponse<object>) HandleKeyNotFoundException(
        KeyNotFoundException exception)
        => (HttpStatusCode.NotFound, new ApiResponse<object>
        {
            StatusCode = HttpStatusCode.NotFound,
            Succeeded = false,
            Message = exception.Message
        });

    private static (HttpStatusCode, ApiResponse<object>) HandleArgumentException(
        ArgumentException exception)
        => (HttpStatusCode.BadRequest, new ApiResponse<object>
        {
            StatusCode = HttpStatusCode.BadRequest,
            Succeeded = false,
            Message = exception.Message
        });

    private static (HttpStatusCode, ApiResponse<object>) HandleInvalidOperationException(
        InvalidOperationException exception)
        => (HttpStatusCode.BadRequest, new ApiResponse<object>
        {
            StatusCode = HttpStatusCode.BadRequest,
            Succeeded = false,
            Message = exception.Message
        });

    private static (HttpStatusCode, ApiResponse<object>) HandleUnknownException(Exception _)
        => (HttpStatusCode.InternalServerError, new ApiResponse<object>
        {
            StatusCode = HttpStatusCode.InternalServerError,
            Succeeded = false,
            Message = "An unexpected error occurred. Please try again later."
        });
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder app)
        => app.UseMiddleware<ExceptionHandlingMiddleware>();
}
