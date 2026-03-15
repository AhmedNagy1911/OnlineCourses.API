using Microsoft.EntityFrameworkCore;
using OnlineCourses.Application.Interfaces.Repositories;
using OnlineCourses.Application.Services;
using OnlineCourses.Infrastructur.Data;
using OnlineCourses.Infrastructur.Repositories;
namespace OnlineCourses.API;

public static class DependencyInjection
{
    public static IServiceCollection AddDependeies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddOpenApi();

        var connectionString = configuration.GetConnectionString("DefaultConnection") ??
    throw new InvalidOperationException("Connection String 'DefaultConnection' not found");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<ICourseRepository , CourseRepository>();


        services
        .AddSwaggerServices();
        
        return services;
    }
    public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        // services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        return services;
    }
}
