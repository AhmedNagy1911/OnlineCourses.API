using Microsoft.AspNetCore.Identity;
using OnlineCourses.API;
using OnlineCourses.Domain.Constants;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

//builder.AddSerilogLogging();
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration)
);

builder.Services.AddDependencies(builder.Configuration);

var app = builder.Build();
await SeedRolesAsync(app);

// ? ????? Global Exception Handling Middleware ???? ?????? ???? ??ResultExtensions 
//app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

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