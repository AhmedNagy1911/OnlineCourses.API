using Microsoft.AspNetCore.Identity;

namespace OnlineCourses.Infrastructur.Persistence;
public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}