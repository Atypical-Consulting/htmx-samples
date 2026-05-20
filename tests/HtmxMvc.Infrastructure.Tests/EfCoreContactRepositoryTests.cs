using HtmxMvc.Domain;
using HtmxMvc.Infrastructure.Contacts;
using Xunit;

namespace HtmxMvc.Infrastructure.Tests;

public class EfCoreContactRepositoryTests
{
    [Fact]
    public async Task AddAsync_assigns_id_and_persists()
    {
        await using var ctx = new SqliteTestContext();
        var repo = new EfCoreContactRepository(ctx.Db);

        var created = await repo.AddAsync(new Contact { Name = "Test", Email = "t@x", Phone = "" });

        Assert.True(created.Id > 0);
        var all = await repo.GetAllAsync();
        Assert.Single(all);
    }

    [Fact]
    public async Task GetAllAsync_returns_contacts_ordered_by_id()
    {
        await using var ctx = new SqliteTestContext();
        var repo = new EfCoreContactRepository(ctx.Db);
        await repo.AddAsync(new Contact { Name = "Alice", Email = "a@x", Phone = "" });
        await repo.AddAsync(new Contact { Name = "Bob",   Email = "b@x", Phone = "" });
        await repo.AddAsync(new Contact { Name = "Carol", Email = "c@x", Phone = "" });

        var all = await repo.GetAllAsync();

        Assert.Equal(new[] { "Alice", "Bob", "Carol" }, all.Select(c => c.Name));
    }

    [Fact]
    public async Task GetAsync_returns_null_when_not_found()
    {
        await using var ctx = new SqliteTestContext();
        var repo = new EfCoreContactRepository(ctx.Db);

        var found = await repo.GetAsync(999);

        Assert.Null(found);
    }

    [Fact]
    public async Task GetAsync_returns_existing()
    {
        await using var ctx = new SqliteTestContext();
        var repo = new EfCoreContactRepository(ctx.Db);
        var created = await repo.AddAsync(new Contact { Name = "Test", Email = "t@x", Phone = "" });

        var found = await repo.GetAsync(created.Id);

        Assert.NotNull(found);
        Assert.Equal("Test", found!.Name);
    }

    [Fact]
    public async Task UpdateAsync_modifies_existing_and_returns_updated()
    {
        await using var ctx = new SqliteTestContext();
        var repo = new EfCoreContactRepository(ctx.Db);
        var created = await repo.AddAsync(new Contact { Name = "Old", Email = "old@x", Phone = "555-0000" });

        var updated = await repo.UpdateAsync(created.Id, new Contact { Name = "New", Email = "new@x", Phone = "555-1111" });

        Assert.NotNull(updated);
        Assert.Equal("New", updated!.Name);
        var reread = await repo.GetAsync(created.Id);
        Assert.Equal("new@x", reread!.Email);
    }

    [Fact]
    public async Task UpdateAsync_returns_null_when_not_found()
    {
        await using var ctx = new SqliteTestContext();
        var repo = new EfCoreContactRepository(ctx.Db);

        var updated = await repo.UpdateAsync(999, new Contact { Name = "X" });

        Assert.Null(updated);
    }

    [Fact]
    public async Task DeleteAsync_removes_existing_and_returns_true()
    {
        await using var ctx = new SqliteTestContext();
        var repo = new EfCoreContactRepository(ctx.Db);
        var created = await repo.AddAsync(new Contact { Name = "Doomed", Email = "d@x", Phone = "" });

        var result = await repo.DeleteAsync(created.Id);

        Assert.True(result);
        Assert.Null(await repo.GetAsync(created.Id));
    }

    [Fact]
    public async Task DeleteAsync_returns_false_when_not_found()
    {
        await using var ctx = new SqliteTestContext();
        var repo = new EfCoreContactRepository(ctx.Db);

        Assert.False(await repo.DeleteAsync(999));
    }
}
