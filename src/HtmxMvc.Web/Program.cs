using HtmxMvc;
using HtmxMvc.Features.Contacts;
using HtmxMvc.Features.Dashboard;
using TheAppManager.Startup;

AppManager.Start(args, modules => modules
    .Add<WebHostModule>()
    .Add<InfrastructureModule>()
    .Add<ContactsModule>()
    .Add<DashboardModule>());
