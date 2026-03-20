namespace ECommerceCenter.Application.Abstractions.Identity;

public sealed record FirebaseIdentity(
    string Uid,
    string Email,
    bool EmailVerified,
    string? DisplayName,
    string? PhotoUrl);

public interface IFirebaseTokenVerifier
{
    Task<FirebaseIdentity?> VerifyIdTokenAsync(
        string idToken,
        CancellationToken cancellationToken = default);
}
