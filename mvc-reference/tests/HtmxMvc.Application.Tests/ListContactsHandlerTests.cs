using HtmxMvc.Application.Contacts;
using HtmxMvc.Domain;
using Xunit;

namespace HtmxMvc.Application.Tests;

public class ListContactsHandlerTests
{
    [Fact]
    public async Task Returns_all_contacts_in_id_order()
    {
        var repo = new FakeContactRepository(
            new Contact { Name = "Bea" },
            new Contact { Name = "Ana" });
        var handler = new ListContactsHandler(repo);

        var result = await handler.ExecuteAsync();

        Assert.Equal(new[] { "Bea", "Ana" }, result.Select(c => c.Name));
    }

    [Fact]
    public async Task Returns_empty_when_repo_is_empty()
    {
        var handler = new ListContactsHandler(new FakeContactRepository());
        var result = await handler.ExecuteAsync();
        Assert.Empty(result);
    }
}
