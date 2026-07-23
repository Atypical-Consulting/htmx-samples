using HtmxMvc.Application.Contacts;
using HtmxMvc.Domain;
using Xunit;

namespace HtmxMvc.Application.Tests;

public class DeleteContactHandlerTests
{
    [Fact]
    public async Task Deletes_existing_contact()
    {
        var repo = new FakeContactRepository(new Contact { Name = "Doomed" });
        var handler = new DeleteContactHandler(repo);

        var deleted = await handler.ExecuteAsync(1);

        Assert.True(deleted);
        Assert.Empty(await repo.GetAllAsync());
    }

    [Fact]
    public async Task Returns_false_when_id_not_found()
    {
        var repo = new FakeContactRepository();
        var handler = new DeleteContactHandler(repo);

        var deleted = await handler.ExecuteAsync(99);

        Assert.False(deleted);
    }
}
