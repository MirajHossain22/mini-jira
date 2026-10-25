using Microsoft.AspNetCore.Mvc;
using TaskTracker.Api.DTOs;
using TaskTracker.Api.Services;

namespace TaskTracker.Api.Controllers;

[ApiController, Route("api/auth")]
public sealed class AuthController(IAuthService auth) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct) => Ok(await auth.RegisterAsync(request, ct));
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct) => Ok(await auth.LoginAsync(request, ct));
}
