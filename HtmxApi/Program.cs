var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS policy for the client-side Blazor app
app.UseCors(policy =>
{
    policy.AllowAnyHeader();
    policy.AllowAnyMethod();
    policy.WithOrigins("http://localhost:5299");
});

app.UseHttpsRedirection();

app.MapGet("/contact/1",
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

app.MapGet("/contact/1/edit",
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

app.MapGet("/sample-content",
    () => """
          <div x-data="{ open: false }">
              <button @click="open = true">Expand</button>
              <span x-show="open">Content...</span>
          </div>
          """);

app.MapGet("/counter",
    () => """
          <div x-data="{ count: 0 }">
              <button x-on:click="count++">Increment</button>
              <span x-text="count"></span>
          </div>
          """);

app.MapGet("/search-input",
    () => """
          <div
              x-data="{
                  search: '',
           
                  items: ['foo', 'bar', 'baz'],
           
                  get filteredItems() {
                      return this.items.filter(
                          i => i.startsWith(this.search)
                      )
                  }
              }"
          >
              <input x-model="search" placeholder="Search...">
           
              <ul>
                  <template x-for="item in filteredItems" :key="item">
                      <li x-text="item"></li>
                  </template>
              </ul>
          </div>
          """);

app.Run();
