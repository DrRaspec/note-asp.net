using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotesApi.Contracts.Auth;
using NotesApi.Models;
using NotesApi.Repositories;
using NotesApi.Services;

namespace NotesApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordService passwordService,
    ITokenService tokenService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        var existingUser = await userRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant());
        if (existingUser is not null)
        {
            return Conflict(new { message = "Email is already registered." });
        }

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = passwordService.HashPassword(user, request.Password);

        user.Id = await userRepository.CreateAsync(user);

        return Ok(await BuildAuthResponseAsync(user));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await userRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant());
        if (user is null || !passwordService.VerifyPassword(user, user.PasswordHash, request.Password))
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        return Ok(await BuildAuthResponseAsync(user));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshRequest request)
    {
        var existingToken = await refreshTokenRepository.GetByTokenAsync(request.RefreshToken.Trim());
        if (existingToken is null || existingToken.RevokedAt is not null || existingToken.ExpiresAt <= DateTime.UtcNow)
        {
            return Unauthorized(new { message = "Invalid refresh token." });
        }

        var user = await userRepository.GetByIdAsync(existingToken.UserId);
        if (user is null)
        {
            return Unauthorized(new { message = "Invalid refresh token user." });
        }

        var auth = await BuildAuthResponseAsync(user);
        await refreshTokenRepository.RevokeAsync(existingToken.Id, DateTime.UtcNow, auth.RefreshToken);
        return Ok(auth);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(claimValue, out var userId))
        {
            return Unauthorized();
        }

        await refreshTokenRepository.RevokeAllForUserAsync(userId, DateTime.UtcNow);
        return NoContent();
    }

    private async Task<AuthResponse> BuildAuthResponseAsync(User user)
    {
        var refreshToken = tokenService.CreateRefreshToken();
        await refreshTokenRepository.CreateAsync(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken.Token,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = refreshToken.ExpiresAt
        });

        return new AuthResponse
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            Token = tokenService.CreateToken(user),
            RefreshToken = refreshToken.Token
        };
    }
}
