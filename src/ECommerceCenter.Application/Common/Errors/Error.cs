namespace ECommerceCenter.Application.Common.Errors;

public class Error
{
    public string Code { get; }
    public string Message { get; }

    public Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public static readonly Error None = new("", "");

    // ── Not Found ──────────────────────────────────────────────────────────
    public static Error NotFound(string message) =>
        new("NOT_FOUND", message);

    public static Error NotFound(string entityName, object id) =>
        new("NOT_FOUND", $"{entityName} with ID '{id}' was not found.");

    // ── Conflict / Duplicate ───────────────────────────────────────────────
    public static Error Conflict(string message) =>
        new("CONFLICT", message);

    public static Error Duplicate(string field) =>
        new("DUPLICATE", $"A record with the same '{field}' already exists.");

    public static Error Duplicate(string field, object value) =>
        new("DUPLICATE", $"A record with {field} = '{value}' already exists.");

    // ── Validation ─────────────────────────────────────────────────────────
    public static Error Validation(string message) =>
        new("VALIDATION_ERROR", message);

    public static Error Validation(string field, string message) =>
        new($"VALIDATION_{field.ToUpperInvariant()}", message);

    // ── Concurrency ────────────────────────────────────────────────────────
    public static Error Concurrency(string message) =>
        new("CONCURRENCY_CONFLICT", message);

    // ── Auth ───────────────────────────────────────────────────────────────
    public static Error Unauthorized(string message = "Authentication is required") =>
        new("UNAUTHORIZED", message);

    public static Error Forbidden(string message = "You are not authorized to perform this action") =>
        new("FORBIDDEN", message);

    // ── Business / Operation ───────────────────────────────────────────────
    public static Error BusinessRule(BusinessRuleCode code, string message) =>
        new($"BUSINESS_RULE_{code.ToString().ToUpperInvariant()}", message);

    public static Error InvalidOperation(string message) =>
        new("INVALID_OPERATION", message);

    // ── System ─────────────────────────────────────────────────────────────
    public static Error ServiceUnavailable(string serviceName) =>
        new("SERVICE_UNAVAILABLE", $"{serviceName} is currently unavailable.");

    public static Error Internal(string message = "An unexpected error occurred") =>
        new("INTERNAL_ERROR", message);
}
