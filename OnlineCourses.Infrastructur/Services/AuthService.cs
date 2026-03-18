using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OnlineCourses.Application.Auth.DTOs;
using OnlineCourses.Application.Interfaces.Services;
using OnlineCourses.Domain.Common;
using OnlineCourses.Domain.Common.Errors;
using OnlineCourses.Domain.Entities;
using OnlineCourses.Infrastructur.Persistence;
using OnlineCourses.Infrastructur.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace OnlineCourses.Infrastructur.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    IOptions<JwtSettings> jwtOptions,
    IEmailService emailService,
    IConfiguration configuration) : IAuthService
{
    private readonly JwtSettings _jwt = jwtOptions.Value;
    private readonly string _baseUrl = configuration["AppSettings:BaseUrl"]!;

    // ── Register ─────────────────────────────────────────────────────
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

        var createResult = await userManager.CreateAsync(user, request.Password);

        if (!createResult.Succeeded)
        {
            var error = new Error(
                "Auth.RegistrationFailed",
                createResult.Errors.First().Description,
                StatusCodes.Status400BadRequest);

            return Result.Failure<AuthResponse>(error);
        }

        await SendConfirmationEmailAsync(user, ct);

        return await BuildAuthResponseAsync(user);
    }

    // ── Login ─────────────────────────────────────────────────────────
    public async Task<Result<AuthResponse>> LoginAsync(
        LoginRequest request, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            return Result.Failure<AuthResponse>(AuthErrors.InvalidCredentials);

        if (!user.EmailConfirmed)
            return Result.Failure<AuthResponse>(AuthErrors.EmailNotConfirmed);

        user.RefreshTokens.RemoveAll(t => !t.IsActive);
        await userManager.UpdateAsync(user);

        return await BuildAuthResponseAsync(user);
    }

    // ── Refresh Token ─────────────────────────────────────────────────
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

        refreshToken.RevokedOn = DateTime.UtcNow;

        return await BuildAuthResponseAsync(user);
    }

    // ── Revoke Token ──────────────────────────────────────────────────
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

    // ── Confirm Email ─────────────────────────────────────────────────
    public async Task<Result> ConfirmEmailAsync(
        string userId, string token, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return Result.Failure(AuthErrors.InvalidOrExpiredToken);

        if (user.EmailConfirmed)
            return Result.Failure(AuthErrors.EmailAlreadyConfirmed);

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        var result = await userManager.ConfirmEmailAsync(user, decodedToken);

        return result.Succeeded
            ? Result.Success()
            : Result.Failure(AuthErrors.InvalidOrExpiredToken);
    }

    // ── Resend Confirmation Email ─────────────────────────────────────
    public async Task<Result> ResendConfirmationEmailAsync(
        ResendConfirmationEmailRequest request, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        // لا تكشف إذا الإيميل موجود أو لأ — دايمًا Success
        if (user is null || user.EmailConfirmed)
            return Result.Success();

        await SendConfirmationEmailAsync(user, ct);

        return Result.Success();
    }

    // ── Forgot Password ───────────────────────────────────────────────
    public async Task<Result> ForgotPasswordAsync(
        ForgotPasswordRequest request, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        // دايمًا Success — لا تكشف وجود الإيميل
        if (user is null || !user.EmailConfirmed)
            return Result.Success();

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var resetLink = $"{_baseUrl}/api/auth/reset-password" +
                           $"?userId={user.Id}&token={encodedToken}";

        await emailService.SendAsync(
            user.Email!,
            "Reset your password",
            EmailTemplates.ResetPassword(user.FirstName, resetLink),
            ct
        );

        return Result.Success();
    }

    // ── Reset Password ────────────────────────────────────────────────
    public async Task<Result> ResetPasswordAsync(
        ResetPasswordRequest request, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(request.UserId);
        if (user is null)
            return Result.Failure(AuthErrors.InvalidOrExpiredToken);

        var decodedToken = Encoding.UTF8.GetString(
                               WebEncoders.Base64UrlDecode(request.Token));

        var result = await userManager.ResetPasswordAsync(
                         user, decodedToken, request.NewPassword);

        return result.Succeeded
            ? Result.Success()
            : Result.Failure(AuthErrors.InvalidOrExpiredToken);
    }

    // ── Private Helpers ───────────────────────────────────────────────
    private async Task SendConfirmationEmailAsync(
        ApplicationUser user, CancellationToken ct)
    {
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var confirmLink = $"{_baseUrl}/api/auth/confirm-email" +
                           $"?userId={user.Id}&token={encodedToken}";

        await emailService.SendAsync(
            user.Email!,
            "Confirm your email",
            EmailTemplates.ConfirmEmail(user.FirstName, confirmLink),
            ct
        );
    }

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