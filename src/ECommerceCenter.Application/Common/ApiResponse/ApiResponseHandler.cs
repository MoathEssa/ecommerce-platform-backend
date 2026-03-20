using System.Net;

namespace ECommerceCenter.Application.Common.ApiResponse;

public static class ApiResponseHandler
{
    public static ApiResponse<T> Success<T>(T data, object? meta = null, string message = "Operation completed successfully.")
        => new() { Data = data, StatusCode = HttpStatusCode.OK, Succeeded = true, Message = message, Meta = meta };

    public static ApiResponse<T> Success<T>(string message = "Operation completed successfully.", object? meta = null)
        => new() { StatusCode = HttpStatusCode.OK, Succeeded = true, Message = message, Meta = meta };

    public static ApiResponse<T> Created<T>(T entity, object? meta = null, string message = "Created successfully.")
        => new() { Data = entity, StatusCode = HttpStatusCode.Created, Succeeded = true, Message = message, Meta = meta };

    public static ApiResponse<T> Deleted<T>(string message = "Deleted successfully.")
        => new() { StatusCode = HttpStatusCode.OK, Succeeded = true, Message = message };

    public static ApiResponse<T> NotFound<T>(string message = "Not found.")
        => new() { StatusCode = HttpStatusCode.NotFound, Succeeded = false, Message = message };

    public static ApiResponse<T> Conflict<T>(string message = "Conflict.")
        => new() { StatusCode = HttpStatusCode.Conflict, Succeeded = false, Message = message };

    public static ApiResponse<T> BadRequest<T>(string message = "Bad request.")
        => new() { StatusCode = HttpStatusCode.BadRequest, Succeeded = false, Message = message };

    public static ApiResponse<T> BadRequest<T>(List<string> errors, string message = "Validation failed.")
        => new() { StatusCode = HttpStatusCode.BadRequest, Succeeded = false, Message = message, Errors = errors };

    public static ApiResponse<T> Unauthorized<T>(string message = "Unauthorized.")
        => new() { StatusCode = HttpStatusCode.Unauthorized, Succeeded = false, Message = message };

    public static ApiResponse<T> Forbidden<T>(string message = "Forbidden.")
        => new() { StatusCode = HttpStatusCode.Forbidden, Succeeded = false, Message = message };

    public static ApiResponse<T> InternalServerError<T>(string message = "An unexpected error occurred.")
        => new() { StatusCode = HttpStatusCode.InternalServerError, Succeeded = false, Message = message };

    public static ApiResponse<T> ServiceUnavailable<T>(string message = "Service unavailable.")
        => new() { StatusCode = HttpStatusCode.ServiceUnavailable, Succeeded = false, Message = message };
}
