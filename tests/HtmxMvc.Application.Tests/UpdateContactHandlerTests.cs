using HtmxMvc.Application.Contacts;
using HtmxMvc.Domain;
using Xunit;

namespace HtmxMvc.Application.Tests;

public class UpdateContactHandlerTests
{
    [Fact]
    public async Task Updates_existing_contact()
    {
        var repo = new FakeContactRepository(
            new Contact { Name = "Old", Email = "old@x.com", Phone = "1" });
        var handler = new UpdateContactHandler(repo);

        var updated = await handler.ExecuteAsync(1, new ContactInput
        {
            Name = "New",
            Email = "new@x.com",
            Phone = "2"
        });

        Assert.NotNull(updated);
        Assert.Equal("New", updated!.Name);
        Assert.Equal("new@x.com", updated.Email);
        Assert.Equal("2", updated.Phone);
    }

    [Fact]
    public async Task Returns_null_when_id_not_found()
    {
        var repo = new FakeContactRepository();
        var handler = new UpdateContactHandler(repo);

        var result = await handler.ExecuteAsync(99, new ContactInput { Name = "x" });

        Assert.Null(result);
    }
}
