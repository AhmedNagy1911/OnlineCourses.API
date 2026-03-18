namespace OnlineCourses.Application.Auth.DTOs;

public record AuthResponse(
    string UserId,
    string Email,
    string DisplayName,
    string Token,
    DateTime TokenExpiration,
    string RefreshToken,
    DateTime RefreshTokenExpiration
);