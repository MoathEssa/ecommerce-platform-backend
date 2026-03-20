namespace ECommerceCenter.Application.Abstractions.DTOs.Auth;

public record LoginRequest(string Email, string Password);

public record RegisterRequest(string Email, string Password, string ConfirmPassword);

public record PersonDto(
    string FirstName,
    string LastName,
    string? Phone,
    DateOnly? DateOfBirth,
    string? AvatarUrl,
    string? Gender);

public record AuthResponse(
    int UserId,
    string Email,
    IList<string> Roles,
    string? FirstName = null,
    string? LastName = null);

/// <summary>Internal transfer object that carries both public response + tokens.</summary>
public record AuthResultInternal(
    AuthResponse User,
    string AccessToken,
    string RefreshToken,
    DateTime RefreshTokenExpiration);

/// <summary>What is returned to the client (refresh token goes in HttpOnly cookie).</summary>
public record LoginResponse(
    AuthResponse User,
    string AccessToken);

public record CurrentUserResponse(
    int UserId,
    string Email,
    IList<string> Roles,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    PersonDto? Person);
