using TravelAgency.Domain.Common;
using TravelAgency.Domain.Enums;

namespace TravelAgency.Domain.Entities;

public sealed class User : BaseEntity
{
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiryTimeUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private User() { }

    public User(string fullName, string email, string passwordHash, UserRole role)
    {
        FullName = fullName;
        Email = email.ToLowerInvariant().Trim();
        PasswordHash = passwordHash;
        Role = role;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void SetRefreshToken(string refreshToken, DateTime expiryTimeUtc)
    {
        RefreshToken = refreshToken;
        RefreshTokenExpiryTimeUtc = expiryTimeUtc;
    }
}