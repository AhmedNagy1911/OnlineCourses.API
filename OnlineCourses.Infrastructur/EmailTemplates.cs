namespace OnlineCourses.Infrastructur;

internal static class EmailTemplates
{
    public static string ConfirmEmail(string firstName, string confirmUrl) => $"""
        <div style="font-family:sans-serif;max-width:520px;margin:auto">
          <h2>Hi {firstName},</h2>
          <p>Thanks for registering! Click the button below to confirm your email.</p>
          <a href="{confirmUrl}"
             style="display:inline-block;padding:12px 28px;background:#4f46e5;
                    color:#fff;border-radius:6px;text-decoration:none;font-weight:600">
            Confirm email
          </a>
          <p style="color:#888;font-size:13px;margin-top:24px">
            Link expires in 24 hours. If you didn't register, ignore this email.
          </p>
        </div>
        """;

    public static string ResetPassword(string firstName, string resetUrl) => $"""
        <div style="font-family:sans-serif;max-width:520px;margin:auto">
          <h2>Hi {firstName},</h2>
          <p>We received a request to reset your password.</p>
          <a href="{resetUrl}"
             style="display:inline-block;padding:12px 28px;background:#dc2626;
                    color:#fff;border-radius:6px;text-decoration:none;font-weight:600">
            Reset password
          </a>
          <p style="color:#888;font-size:13px;margin-top:24px">
            Link expires in 1 hour. If you didn't request this, ignore this email.
          </p>
        </div>
        """;
}