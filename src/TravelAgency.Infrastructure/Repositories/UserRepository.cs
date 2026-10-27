using Microsoft.EntityFrameworkCore;
using TravelAgency.Application.Interfaces;
using TravelAgency.Domain.Entities;
using TravelAgency.Infrastructure.Persistence;

namespace TravelAgency.Infrastructure.Repositories;

public sealed class UserRepository(TravelAgencyDbContext context) : IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalized = email.ToLowerInvariant().Trim();
        return await context.Users.FirstOrDefaultAsync(u => u.Email == normalized, ct);
    }

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken, ct);
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await context.Users.AddAsync(user, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await context.SaveChangesAsync(ct);
    }
}