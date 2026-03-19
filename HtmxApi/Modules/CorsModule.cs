using TheAppManager.Modules;

namespace HtmxApi.Modules;

public class CorsModule : IAppModule
{
    public void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddCors();
    }

    public void ConfigureMiddleware(WebApplication app)
    {
        // CORS policy for the client-side Blazor app
        app.UseCors(policy =>
        {
            policy.AllowAnyHeader();
            policy.AllowAnyMethod();
            policy.WithOrigins("http://localhost:5299");
        });

        app.UseHttpsRedirection();
    }
}
