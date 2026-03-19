using TheAppManager.Modules;

namespace HtmxApi.Modules;

public class AlpineEndpointsModule : IAppModule
{
    public void ConfigureEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/sample-content",
            () => """
                  <div x-data="{ open: false }">
                      <button @click="open = true">Expand</button>
                      <span x-show="open">Content...</span>
                  </div>
                  """);

        endpoints.MapGet("/counter",
            () => """
                  <div x-data="{ count: 0 }">
                      <button x-on:click="count++">Increment</button>
                      <span x-text="count"></span>
                  </div>
                  """);

        endpoints.MapGet("/search-input",
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
    }
}
