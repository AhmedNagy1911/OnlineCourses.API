using Microsoft.AspNetCore.Identity;
using OnlineCourses.Application.Common.Interfaces;
using OnlineCourses.Infrastructur.Persistence;

namespace OnlineCourses.Infrastructur.Services;

public class UserService(UserManager<ApplicationUser> userManager) : IUserService
{
    public async Task<bool> ExistsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        return user is not null;
    }

    public async Task<(string Id, string FullName)?> GetUserInfoAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return null;

        return (user.Id, $"{user.FirstName} {user.LastName}");
    }
}