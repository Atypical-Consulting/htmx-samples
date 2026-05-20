using HtmxMvc.Application.Dashboard;
using TheAppManager.Modules;

namespace HtmxMvc.Features.Dashboard;

public sealed class DashboardModule : IAppModule
{
    public void ConfigureServices(WebApplicationBuilder builder)
        => builder.Services.AddScoped<GetDashboardStatsHandler>();
}
