using Moq;
using TaskTracker.Api.Common;
using TaskTracker.Api.DTOs;
using TaskTracker.Api.Entities;
using TaskTracker.Api.Repositories;
using TaskTracker.Api.Services;

namespace TaskTracker.Tests;

public sealed class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<ITokenService> _tokens = new();

    [Fact]
    public async Task Register_HashesPasswordAndReturnsToken()
    {
        _users.Setup(x => x.GetByEmailAsync("dev@example.com", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        _tokens.Setup(x => x.Create(It.IsAny<Guid>(), "dev@example.com")).Returns("jwt");
        var service = new AuthService(_users.Object, _tokens.Object);
        var result = await service.RegisterAsync(new RegisterRequest("DEV@example.com", "password123"));
        Assert.Equal("jwt", result.Token);
        _users.Verify(x => x.AddAsync(It.Is<User>(u => u.Email == "dev@example.com" && u.PasswordHash != "password123"), It.IsAny<CancellationToken>()));
        _users.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task Register_WhenEmailExists_ThrowsConflict()
    {
        _users.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new User { Email = "dev@example.com", PasswordHash = "hash" });
        var service = new AuthService(_users.Object, _tokens.Object);
        await Assert.ThrowsAsync<ConflictException>(() => service.RegisterAsync(new RegisterRequest("dev@example.com", "password123")));
    }

    [Fact]
    public async Task Login_WithWrongPassword_ThrowsUnauthorized()
    {
        _users.Setup(x => x.GetByEmailAsync("dev@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(new User { Email = "dev@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("right-password") });
        var service = new AuthService(_users.Object, _tokens.Object);
        await Assert.ThrowsAsync<UnauthorizedException>(() => service.LoginAsync(new LoginRequest("dev@example.com", "wrong-password")));
    }
}
