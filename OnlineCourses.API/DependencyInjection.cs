using OnlineCourses.Application;
using OnlineCourses.Infrastructur;
namespace OnlineCourses.API;

public static class DependencyInjection
{
    public static IServiceCollection AddDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddOpenApi();

        services.AddApplication();
        services.AddInfrastructure(configuration);

        services.AddSwaggerServices();

        return services;
    }

    public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
     
        return services;
    }
}