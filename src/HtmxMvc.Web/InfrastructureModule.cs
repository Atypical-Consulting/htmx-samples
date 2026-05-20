using HtmxMvc.Infrastructure;
using TheAppManager.Modules;

namespace HtmxMvc;

public sealed class InfrastructureModule : IAppModule
{
    public void ConfigureServices(WebApplicationBuilder builder)
        => builder.Services.AddInfrastructure();
}
