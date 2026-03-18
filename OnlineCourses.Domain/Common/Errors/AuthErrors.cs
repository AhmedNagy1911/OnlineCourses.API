using OnlineCourses.Domain.Common;

namespace OnlineCourses.Domain.Common.Errors;

public static class AuthErrors
{
    public static readonly Error InvalidCredentials = new(
        Code: "Auth.InvalidCredentials",
        Description: "Invalid email or password.",
        StatusCode: 401
    );

    public static readonly Error DuplicateEmail = new(
        Code: "Auth.DuplicateEmail",
        Description: "A user with this email already exists.",
        StatusCode: 409
    );

    public static readonly Error UserNotFound = new(
        Code: "Auth.UserNotFound",
        Description: "User not found.",
        StatusCode: 404
    );

    public static readonly Error RoleNotFound = new(
        Code: "Auth.RoleNotFound",
        Description: "Role does not exist.",
        StatusCode: 400
    );

    public static Error RegistrationFailed(IEnumerable<string> errors) => new(
        Code: "Auth.RegistrationFailed",
        Description: string.Join(", ", errors),
        StatusCode: 400
    );
}