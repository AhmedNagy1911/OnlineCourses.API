using Microsoft.AspNetCore.Identity;
using OnlineCourses.API;
using OnlineCourses.Application;
using OnlineCourses.Domain.Constants;
using OnlineCourses.Infrastructur;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependencies(builder.Configuration);

//builder.Services.AddApplication();      // Application Layer DI
//builder.Services.AddInfrastructure(builder.Configuration);
var app = builder.Build();
await SeedRolesAsync(app);
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
static async Task SeedRolesAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles = [AppRoles.Admin, AppRoles.Student, AppRoles.Instructor];

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}