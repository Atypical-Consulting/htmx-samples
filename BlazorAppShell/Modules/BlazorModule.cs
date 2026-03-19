using BlazorAppShell.Components;
using BlazorAppShell.Services;
using TheAppManager.Modules;

namespace BlazorAppShell.Modules;

public class BlazorModule : IAppModule
{
    public void ConfigureServices(WebApplicationBuilder builder)
    {
        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        // Add HttpClient and HtmxClient
        builder.Services.AddHttpClient<HtmxClient>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:5164/");
        });
    }

    public void ConfigureMiddleware(WebApplication app)
    {
        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();
        app.UseAntiforgery();
    }

    public void ConfigureEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();
    }
}
