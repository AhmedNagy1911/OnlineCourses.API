using OnlineCourses.Application.Auth.DTOs;
using OnlineCourses.Domain.Common;

namespace OnlineCourses.Application.Interfaces.Services;

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<Result<AuthResponse>> RefreshTokenAsync(string token, CancellationToken ct = default);
    Task<Result> RevokeTokenAsync(string token, CancellationToken ct = default);

    Task<Result> ConfirmEmailAsync(string userId, string token, CancellationToken ct = default);

    Task<Result> ResendConfirmationEmailAsync(ResendConfirmationEmailRequest request, CancellationToken ct = default);

    Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default);

    Task<Result> ResetPasswordAsync(ResetPasswordRequest request,CancellationToken ct = default);
}