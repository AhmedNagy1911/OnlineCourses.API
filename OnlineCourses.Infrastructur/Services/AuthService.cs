using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OnlineCourses.Application.Auth.DTOs;
using OnlineCourses.Application.Interfaces.Services;
using OnlineCourses.Domain.Common;
using OnlineCourses.Domain.Common.Errors;
using OnlineCourses.Domain.Constants;
using OnlineCourses.Infrastructur.Persistence;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OnlineCourses.Infrastructur.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration) : IAuthService
{
    public async Task<Result<AuthResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        // 1. Check duplicate email
        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
            return Result.Failure<AuthResponse>(AuthErrors.DuplicateEmail);

        // 2. Create user
        var user = new ApplicationUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            UserName = request.Email
        };

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
            return  Result.Failure<AuthResponse>(
                AuthErrors.RegistrationFailed(createResult.Errors.Select(e => e.Description)));

        // 3. Assign default role
        await userManager.AddToRoleAsync(user, AppRoles.Student);

        // 4. Generate token
        return Result<AuthResponse>.Success(await BuildAuthResponseAsync(user));
    }

    public async Task<Result<AuthResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        // 1. Find user
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Result.Failure<AuthResponse>(AuthErrors.InvalidCredentials);

        // 2. Verify password
        var validPassword = await userManager.CheckPasswordAsync(user, request.Password);
        if (!validPassword)
            return Result.Failure<AuthResponse>(AuthErrors.InvalidCredentials);

        // 3. Generate token
        return Result<AuthResponse>.Success(await BuildAuthResponseAsync(user));
    }

    // ─── Private Helpers ────────────────────────────────────────────────────

    private async Task<AuthResponse> BuildAuthResponseAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        var token = GenerateJwtToken(user, roles);
        var expiry = DateTime.UtcNow.AddMinutes(GetExpiryMinutes());

        return new AuthResponse(
            Id: user.Id,
            Email: user.Email!,
            FirstName: user.FirstName,
            LastName: user.LastName,
            Token: token,
            ExpiresAt: expiry,
            Roles: roles
        );
    }

    private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,   user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
            new("firstName", user.FirstName),
            new("lastName",  user.LastName)
        };

        // Add role claims
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(GetExpiryMinutes()),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private int GetExpiryMinutes() =>
        int.TryParse(configuration["JwtSettings:ExpiryMinutes"], out var mins) ? mins : 60;
}