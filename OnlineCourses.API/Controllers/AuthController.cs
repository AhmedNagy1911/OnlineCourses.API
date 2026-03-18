using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OnlineCourses.API.Extensions;
using OnlineCourses.Application.Auth.DTOs;
using OnlineCourses.Application.Interfaces.Services;

namespace OnlineCourses.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController( IAuthService authService,
    IValidator<RegisterRequest> registerValidator,
    IValidator<LoginRequest> loginValidator) : ControllerBase
{
    private readonly IAuthService _authService = authService;

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await registerValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

        var result = await _authService.RegisterAsync(request, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await loginValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

        var result = await _authService.LoginAsync(request, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(
    [FromBody] RefreshTokenRequest request,
    CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshTokenAsync(request.Token, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    [HttpPost("revoke-token")]
    public async Task<IActionResult> RevokeToken(
        [FromBody] RevokeTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.RevokeTokenAsync(request.Token, cancellationToken);
        return result.IsSuccess
            ? Ok()
            : result.ToProblem();
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token, CancellationToken ct)
    {
        var result = await _authService.ConfirmEmailAsync(userId, token, ct);
        return result.IsSuccess ? Ok() : result.ToProblem();
    }

    // ── Resend Confirmation Email ────────────────────────────────────
    [HttpPost("resend-confirmation-email")]
    public async Task<IActionResult> ResendConfirmationEmail(ResendConfirmationEmailRequest request, CancellationToken ct)
    {
        var result = await _authService.ResendConfirmationEmailAsync(request, ct);
        return result.IsSuccess ? Ok() : result.ToProblem();
    }

    // ── Forgot Password ──────────────────────────────────────────────
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request, CancellationToken ct)
    {
        var result = await _authService.ForgotPasswordAsync(request, ct);
        return result.IsSuccess ? Ok() : result.ToProblem();
    }

    // ── Reset Password ───────────────────────────────────────────────
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request,CancellationToken ct)
    {
        var result = await _authService.ResetPasswordAsync(request, ct);
        return result.IsSuccess ? Ok() : result.ToProblem();
    }
}