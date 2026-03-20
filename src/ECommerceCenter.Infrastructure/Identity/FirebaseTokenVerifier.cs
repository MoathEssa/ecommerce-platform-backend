using ECommerceCenter.Application.Abstractions.Identity;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ECommerceCenter.Infrastructure.Identity;

public class FirebaseTokenVerifier(
    IConfiguration configuration,
    ILogger<FirebaseTokenVerifier> logger)
    : IFirebaseTokenVerifier
{
    private readonly object _lock = new();
    private FirebaseAuth? _firebaseAuth;

    public async Task<FirebaseIdentity?> VerifyIdTokenAsync(
        string idToken,
        CancellationToken cancellationToken = default)
    {
        EnsureInitialized();

        if (_firebaseAuth is null)
            return null;

        try
        {
            var decoded = await _firebaseAuth.VerifyIdTokenAsync(idToken);
            var user = await _firebaseAuth.GetUserAsync(decoded.Uid);

            if (string.IsNullOrWhiteSpace(user.Email))
                return null;

            return new FirebaseIdentity(
                user.Uid,
                user.Email,
                user.EmailVerified,
                user.DisplayName,
                user.PhotoUrl);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Firebase ID token verification failed.");
            return null;
        }
    }

    private void EnsureInitialized()
    {
        if (_firebaseAuth is not null)
            return;

        lock (_lock)
        {
            if (_firebaseAuth is not null)
                return;

            var serviceAccountJson = configuration["FirebaseAuth:ServiceAccountJson"];
            if (string.IsNullOrWhiteSpace(serviceAccountJson))
            {
                logger.LogWarning("FirebaseAuth:ServiceAccountJson is not configured.");
                return;
            }

            // DefaultInstance returns null (does NOT throw) when no app is created yet.
            var app = FirebaseApp.DefaultInstance ?? FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromJson(serviceAccountJson)
            });

            _firebaseAuth = FirebaseAuth.GetAuth(app);
        }
    }
}
