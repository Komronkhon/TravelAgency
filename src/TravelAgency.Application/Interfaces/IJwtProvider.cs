using TravelAgency.Domain.Entities;

namespace TravelAgency.Application.Interfaces;

public interface IJwtProvider
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}