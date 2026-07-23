using HtmxApi.Modules;
using TheAppManager.Startup;

AppManager.Start(args, modules =>
{
    modules
        .Add<SwaggerModule>()
        .Add<CorsModule>()
        .Add<ContactEndpointsModule>()
        .Add<AlpineEndpointsModule>();
});
