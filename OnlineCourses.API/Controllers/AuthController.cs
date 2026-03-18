using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OnlineCourses.Application.Auth.DTOs;
using OnlineCourses.Application.Interfaces.Services;
using OnlineCourses.API.Extensions;

namespace OnlineCourses.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController( IAuthService authService,
    IValidator<RegisterRequest> registerValidator,
    IValidator<LoginRequest> loginValidator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await registerValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

        var result = await authService.RegisterAsync(request, cancellationToken);
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

        var result = await authService.LoginAsync(request, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(
    [FromBody] RefreshTokenRequest request,
    CancellationToken cancellationToken)
    {
        var result = await authService.RefreshTokenAsync(request.Token, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    [HttpPost("revoke-token")]
    public async Task<IActionResult> RevokeToken(
        [FromBody] RevokeTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.RevokeTokenAsync(request.Token, cancellationToken);
        return result.IsSuccess
            ? Ok()
            : result.ToProblem();
    }
}