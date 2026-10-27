using TravelAgency.Application.Interfaces;
using TravelAgency.Domain.Common;
using TravelAgency.Domain.Entities;
using TravelAgency.Domain.Enums;

namespace TravelAgency.Application.Auth;

public sealed record RegisterRequest(string FullName, string Email, string Password, string Role = "Customer");
public sealed record LoginRequest(string Email, string Password);
public sealed record RefreshTokenRequest(string RefreshToken);
public sealed record AuthResponse(string AccessToken, string RefreshToken, string Email, string Role);

public interface IAuthApplicationService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default);
}

public sealed class AuthApplicationService(
    IUserRepository userRepository,
    IJwtProvider jwtProvider) : IAuthApplicationService
{
    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
            return Result.Failure<AuthResponse>(Error.Validation("Auth.InvalidEmail", "Некорректный формат Email."));

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            return Result.Failure<AuthResponse>(Error.Validation("Auth.WeakPassword", "Пароль должен содержать минимум 6 символов."));

        var existingUser = await userRepository.GetByEmailAsync(request.Email, ct);
        if (existingUser is not null)
            return Result.Failure<AuthResponse>(Error.Conflict("Auth.EmailExists", "Пользователь с таким Email уже зарегистрирован."));

        if (!Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var role))
        {
            role = UserRole.Customer;
        }

        var passwordHash = jwtProvider.HashPassword(request.Password);
        var user = new User(request.FullName, request.Email, passwordHash, role);

        var accessToken = jwtProvider.GenerateAccessToken(user);
        var refreshToken = jwtProvider.GenerateRefreshToken();
        user.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));

        await userRepository.AddAsync(user, ct);
        await userRepository.SaveChangesAsync(ct);

        return Result.Success(new AuthResponse(accessToken, refreshToken, user.Email, user.Role.ToString()));
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, ct);
        if (user is null || !jwtProvider.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Result.Failure<AuthResponse>(
                Error.Validation("Auth.InvalidCredentials", "Неверный Email или пароль."));
        }

        var accessToken = jwtProvider.GenerateAccessToken(user);
        var refreshToken = jwtProvider.GenerateRefreshToken();
        user.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));

        await userRepository.SaveChangesAsync(ct);

        return Result.Success(new AuthResponse(accessToken, refreshToken, user.Email, user.Role.ToString()));
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default)
    {
        var user = await userRepository.GetByRefreshTokenAsync(request.RefreshToken, ct);
        if (user is null || user.RefreshTokenExpiryTimeUtc <= DateTime.UtcNow)
        {
            return Result.Failure<AuthResponse>(
                Error.Validation("Auth.InvalidRefreshToken", "Refresh-токен недействителен или истёк."));
        }

        var newAccessToken = jwtProvider.GenerateAccessToken(user);
        var newRefreshToken = jwtProvider.GenerateRefreshToken();
        user.SetRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(7));

        await userRepository.SaveChangesAsync(ct);

        return Result.Success(new AuthResponse(newAccessToken, newRefreshToken, user.Email, user.Role.ToString()));
    }
}