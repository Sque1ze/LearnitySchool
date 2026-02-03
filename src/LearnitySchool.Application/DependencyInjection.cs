using Microsoft.Extensions.DependencyInjection;

namespace LearnitySchool.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Here you can add MediatR, validators, AutoMapper, etc.
        return services;
    }
}
