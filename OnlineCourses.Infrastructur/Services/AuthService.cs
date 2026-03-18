using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OnlineCourses.Application.Auth.DTOs;
using OnlineCourses.Application.Interfaces.Services;
using OnlineCourses.Domain.Common;
using OnlineCourses.Domain.Common.Errors;
using OnlineCourses.Domain.Constants;
using OnlineCourses.Domain.Entities;
using OnlineCourses.Infrastructur.Persistence;
using OnlineCourses.Infrastructur.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;

namespace OnlineCourses.Infrastructur.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    IOptions<JwtSettings> jwtOptions) : IAuthService
{
    private readonly JwtSettings _jwt = jwtOptions.Value;

    public async Task<Result<AuthResponse>> RegisterAsync(
        RegisterRequest request, CancellationToken ct = default)
    {
        if (await userManager.FindByEmailAsync(request.Email) is not null)
            return Result.Failure<AuthResponse>(AuthErrors.EmailAlreadyExists);

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var error = new Error(
                "Auth.RegistrationFailed",
                result.Errors.First().Description,
                StatusCodes.Status400BadRequest);

            return Result.Failure<AuthResponse>(error);
        }

        return await BuildAuthResponseAsync(user);
    }

    public async Task<Result<AuthResponse>> LoginAsync(
        LoginRequest request, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            return Result.Failure<AuthResponse>(AuthErrors.InvalidCredentials);

        user.RefreshTokens.RemoveAll(t => !t.IsActive);
        await userManager.UpdateAsync(user);

        return await BuildAuthResponseAsync(user);
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(
        string token, CancellationToken ct = default)
    {
        var user = await userManager.Users
            .SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token), ct);

        if (user is null)
            return Result.Failure<AuthResponse>(AuthErrors.InvalidToken);

        var refreshToken = user.RefreshTokens.Single(t => t.Token == token);

        if (!refreshToken.IsActive)
            return Result.Failure<AuthResponse>(
                refreshToken.IsExpired
                    ? AuthErrors.InvalidToken
                    : AuthErrors.TokenAlreadyRevoked);

        // Rotate: عطّل القديم وولّد جديد
        refreshToken.RevokedOn = DateTime.UtcNow;

        return await BuildAuthResponseAsync(user);
    }

    public async Task<Result> RevokeTokenAsync(
        string token, CancellationToken ct = default)
    {
        var user = await userManager.Users
            .SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token), ct);

        if (user is null)
            return Result.Failure(AuthErrors.InvalidToken);

        var refreshToken = user.RefreshTokens.Single(t => t.Token == token);

        if (!refreshToken.IsActive)
            return Result.Failure(AuthErrors.TokenAlreadyRevoked);

        refreshToken.RevokedOn = DateTime.UtcNow;
        await userManager.UpdateAsync(user);

        return Result.Success();
    }

    // ──────────────────────────────────────────
    // Private Helpers
    // ──────────────────────────────────────────
    private async Task<Result<AuthResponse>> BuildAuthResponseAsync(ApplicationUser user)
    {
        var (jwtToken, jwtExpiry) = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        user.RefreshTokens.Add(refreshToken);
        await userManager.UpdateAsync(user);

        return Result.Success(new AuthResponse(
            UserId: user.Id,
            Email: user.Email!,
            DisplayName: $"{user.FirstName} {user.LastName}",
            Token: jwtToken,
            TokenExpiration: jwtExpiry,
            RefreshToken: refreshToken.Token,
            RefreshTokenExpiration: refreshToken.ExpiresOn
        ));
    }

    private (string Token, DateTime Expiry) GenerateJwtToken(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,   user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
            new("displayName", $"{user.FirstName} {user.LastName}")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddMinutes(_jwt.DurationInMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: expiry,
            signingCredentials: creds
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiry);
    }

    private static RefreshToken GenerateRefreshToken() => new()
    {
        Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
        CreatedOn = DateTime.UtcNow,
        ExpiresOn = DateTime.UtcNow.AddDays(14)
    };
}