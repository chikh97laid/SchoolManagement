using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using System.Reflection;

namespace EduCore.Application.DependencyInjection;

public static class ApplicationServicesRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // read all validators from the current assembly and register them with the DI container
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // you can also register AutoMapper or MediatR here in the future
        services.AddAutoMapper(cfg =>
        {
            cfg.AddMaps(Assembly.GetExecutingAssembly());
        });

        return services;
    }
}