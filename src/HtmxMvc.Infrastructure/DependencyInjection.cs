using HtmxMvc.Domain;
using HtmxMvc.Infrastructure.Contacts;
using Microsoft.Extensions.DependencyInjection;

namespace HtmxMvc.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IContactRepository, InMemoryContactRepository>();
        return services;
    }
}
