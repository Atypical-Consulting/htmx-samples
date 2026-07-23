using HtmxMvc.Application.Contacts;
using Xunit;

namespace HtmxMvc.Application.Tests;

public class AddContactHandlerTests
{
    [Fact]
    public async Task Adds_contact_and_assigns_id()
    {
        var repo = new FakeContactRepository();
        var handler = new AddContactHandler(repo);

        var created = await handler.ExecuteAsync(new ContactInput
        {
            Name = "Linus Torvalds",
            Email = "linus@kernel.org",
            Phone = "555-0106"
        });

        Assert.True(created.Id > 0);
        Assert.Equal("Linus Torvalds", created.Name);

        var all = await repo.GetAllAsync();
        Assert.Single(all);
    }
}
