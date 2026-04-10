namespace OnlineCourses.Application.Common.Interfaces;

public interface IUserService
{
    Task<bool> ExistsAsync(string userId, CancellationToken cancellationToken = default);
    Task<(string Id, string FullName)?> GetUserInfoAsync(string userId, CancellationToken cancellationToken = default);
}