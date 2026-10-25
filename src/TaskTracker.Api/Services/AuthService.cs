using TaskTracker.Api.Common;
using TaskTracker.Api.DTOs;
using TaskTracker.Api.Entities;
using TaskTracker.Api.Repositories;

namespace TaskTracker.Api.Services;

public sealed class AuthService(IUserRepository users, ITokenService tokens) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await users.GetByEmailAsync(email, ct) is not null) throw new ConflictException("Email is already registered.");
        var user = new User { Email = email, PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password) };
        await users.AddAsync(user, ct); await users.SaveChangesAsync(ct);
        return new(tokens.Create(user.Id, user.Email), user.Id, user.Email);
    }
    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await users.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), ct);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)) throw new UnauthorizedException("Invalid email or password.");
        return new(tokens.Create(user.Id, user.Email), user.Id, user.Email);
    }
}
