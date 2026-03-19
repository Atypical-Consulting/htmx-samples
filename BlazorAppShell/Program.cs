using BlazorAppShell.Modules;
using TheAppManager.Startup;

AppManager.Start(args, modules =>
{
    modules
        .Add<BlazorModule>();
});
