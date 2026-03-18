namespace OnlineCourses.Application.Auth.DTOs;

public record ResetPasswordRequest(
    string UserId,
    string Token,
    string NewPassword);