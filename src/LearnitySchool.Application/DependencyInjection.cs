using Microsoft.Extensions.DependencyInjection;

namespace LearnitySchool.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // реєстрації сервісів Application
        return services;
    }
}
