using System.Net;
using ECommerceCenter.Application.Common.Errors;
using static ECommerceCenter.Application.Common.Errors.BusinessRuleCode;

namespace ECommerceCenter.Application.Common.ResultPattern;

// ── Generic result ─────────────────────────────────────────────────────────────
public class Result<T>
{
    public bool IsSuccess { get; }
    public HttpStatusCode StatusCode { get; }
    public T? Value { get; }
    public Error Error { get; }
    public string? Message { get; }

    private Result(T? value, string? message = null)
    {
        Value = value;
        Error = Error.None;
        IsSuccess = true;
        StatusCode = HttpStatusCode.OK;
        Message = message;
    }

    private Result(Error error, HttpStatusCode statusCode)
    {
        Value = default;
        Error = error;
        IsSuccess = false;
        StatusCode = statusCode;
        Message = null;
    }

    // ── Success factories ──────────────────────────────────────────────────
    public static Result<T> Success(T value) => new(value);
    public static Result<T> Success(T value, string message) => new(value, message);

    // ── Failure factories ──────────────────────────────────────────────────
    public static Result<T> Failure(Error error, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        => new(error, statusCode);

    public static Result<T> NotFound(string message)
        => new(Error.NotFound(message), HttpStatusCode.NotFound);

    public static Result<T> NotFound(string entityName, object id)
        => new(Error.NotFound(entityName, id), HttpStatusCode.NotFound);

    public static Result<T> Conflict(string message)
        => new(Error.Conflict(message), HttpStatusCode.Conflict);

    public static Result<T> Duplicate(string field)
        => new(Error.Duplicate(field), HttpStatusCode.Conflict);

    public static Result<T> Duplicate(string field, object value)
        => new(Error.Duplicate(field, value), HttpStatusCode.Conflict);

    public static Result<T> ValidationError(string message)
        => new(Error.Validation(message), HttpStatusCode.BadRequest);

    public static Result<T> ValidationError(string field, string message)
        => new(Error.Validation(field, message), HttpStatusCode.BadRequest);

    public static Result<T> Unauthorized(string message = "Authentication is required")
        => new(Error.Unauthorized(message), HttpStatusCode.Unauthorized);

    public static Result<T> Forbidden(string message = "You are not authorized to perform this action")
        => new(Error.Forbidden(message), HttpStatusCode.Forbidden);

    public static Result<T> BusinessRuleViolation(BusinessRuleCode code, string message)
        => new(Error.BusinessRule(code, message), HttpStatusCode.UnprocessableEntity);

    public static Result<T> InvalidOperation(string message)
        => new(Error.InvalidOperation(message), HttpStatusCode.BadRequest);

    // ── Implicit conversion from T ─────────────────────────────────────────
    public static implicit operator Result<T>(T value) => Success(value);
}

// ── Non-generic result (for void operations) ───────────────────────────────────
public class Result
{
    public bool IsSuccess { get; }
    public HttpStatusCode StatusCode { get; }
    public Error Error { get; }
    public string? Message { get; }

    private Result(string? message = null)
    {
        Error = Error.None;
        IsSuccess = true;
        StatusCode = HttpStatusCode.OK;
        Message = message;
    }

    private Result(Error error, HttpStatusCode statusCode)
    {
        Error = error;
        IsSuccess = false;
        StatusCode = statusCode;
        Message = null;
    }

    public static Result Success() => new();
    public static Result Success(string message) => new(message);

    public static Result Failure(Error error, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        => new(error, statusCode);

    public static Result NotFound(string message)
        => new(Error.NotFound(message), HttpStatusCode.NotFound);

    public static Result NotFound(string entityName, object id)
        => new(Error.NotFound(entityName, id), HttpStatusCode.NotFound);

    public static Result Conflict(string message)
        => new(Error.Conflict(message), HttpStatusCode.Conflict);

    public static Result ValidationError(string message)
        => new(Error.Validation(message), HttpStatusCode.BadRequest);

    public static Result Unauthorized(string message = "Authentication is required")
        => new(Error.Unauthorized(message), HttpStatusCode.Unauthorized);

    public static Result Forbidden(string message = "You are not authorized to perform this action")
        => new(Error.Forbidden(message), HttpStatusCode.Forbidden);

    public static Result BusinessRuleViolation(BusinessRuleCode code, string message)
        => new(Error.BusinessRule(code, message), HttpStatusCode.UnprocessableEntity);
}
