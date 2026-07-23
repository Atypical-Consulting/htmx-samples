using HtmxMvc.Infrastructure;
using TheAppManager.Modules;

namespace HtmxMvc;

public sealed class InfrastructureModule : IAppModule
{
    public void ConfigureServices(WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Default in configuration.");
        builder.Services.AddInfrastructure(connectionString);
    }

    public void ConfigureMiddleware(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
        DatabaseSeeder.Seed(db);
    }
}
