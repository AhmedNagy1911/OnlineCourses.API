namespace OnlineCourses.Infrastructur.Settings;

public sealed class MailSettings
{
    public string Host { get; init; } = string.Empty;
    public int Port { get; init; } = 587;
    public string DisplayName { get; init; } = string.Empty;
    public string From { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public bool EnableSsl { get; init; } = true;
}