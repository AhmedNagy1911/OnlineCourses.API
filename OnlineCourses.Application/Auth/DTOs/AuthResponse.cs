namespace OnlineCourses.Application.Auth.DTOs;

public record AuthResponse(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string Token,
    DateTime ExpiresAt,
    IEnumerable<string> Roles
);