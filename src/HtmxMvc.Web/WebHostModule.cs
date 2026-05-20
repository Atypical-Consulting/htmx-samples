using HtmxMvc.Features;
using Microsoft.AspNetCore.Mvc.Razor;
using TheAppManager.Modules;

namespace HtmxMvc;

public sealed class WebHostModule : IAppModule
{
    public void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddControllersWithViews();
        builder.Services.Configure<RazorViewEngineOptions>(o =>
            o.ViewLocationExpanders.Add(new FeatureViewLocationExpander()));
    }

    public void ConfigureMiddleware(WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthorization();
    }

    public void ConfigureEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapStaticAssets();
        endpoints.MapControllers();
    }
}
