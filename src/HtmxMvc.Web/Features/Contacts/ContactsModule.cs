using HtmxMvc.Application.Contacts;
using TheAppManager.Modules;

namespace HtmxMvc.Features.Contacts;

public sealed class ContactsModule : IAppModule
{
    public void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<ListContactsHandler>();
        builder.Services.AddScoped<SearchContactsHandler>();
        builder.Services.AddScoped<GetContactHandler>();
        builder.Services.AddScoped<AddContactHandler>();
        builder.Services.AddScoped<UpdateContactHandler>();
        builder.Services.AddScoped<DeleteContactHandler>();
    }
}
