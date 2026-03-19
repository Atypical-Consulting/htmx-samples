using TheAppManager.Modules;

namespace HtmxApi.Modules;

public class ContactEndpointsModule : IAppModule
{
    public void ConfigureEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/contact/1",
            () => """
                   <div hx-target="this" hx-swap="outerHTML">
                     <div><label>First Name</label>: Joe</div>
                     <div><label>Last Name</label>: Blow</div>
                     <div><label>Email</label>: joe@blow.com</div>
                     <button hx-get="http://localhost:5164/contact/1/edit" class="btn btn-primary">
                       Click To Edit
                     </button>
                   </div>
                  """)
            .WithName("GetContact")
            .WithOpenApi();

        endpoints.MapGet("/contact/1/edit",
            () => """
                  <form hx-put="/contact/1" hx-target="this" hx-swap="outerHTML">
                    <div>
                      <label>First Name</label>
                      <input type="text" name="firstName" value="Joe">
                    </div>
                    <div class="form-group">
                      <label>Last Name</label>
                      <input type="text" name="lastName" value="Blow">
                    </div>
                    <div class="form-group">
                      <label>Email Address</label>
                      <input type="email" name="email" value="joe@blow.com">
                    </div>
                    <button class="btn">Submit</button>
                    <button class="btn" hx-get="/contact/1">Cancel</button>
                  </form>
                  """)
            .WithName("GetContactEdit")
            .WithOpenApi();
    }
}
