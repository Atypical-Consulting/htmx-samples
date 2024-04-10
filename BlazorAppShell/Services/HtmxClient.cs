namespace BlazorAppShell.Services;

public class HtmxClient(HttpClient httpClient)
{
    public async Task<string> GetContactAsync()
        => await GetAsync("contact/1/");

    public async Task<string> GetContactEditAsync()
        => await GetAsync("contact/1/edit/");

    public async Task<string> GetSampleContentAsync()
        => await GetAsync("sample-content");
    
    public async Task<string> GetCounterAsync()
        => await GetAsync("counter");
    
    public async Task<string> GetSearchInputAsync()
        => await GetAsync("search-input");

    private async Task<string> GetAsync(string route)
    {
        HttpResponseMessage response = await httpClient.GetAsync(route);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}