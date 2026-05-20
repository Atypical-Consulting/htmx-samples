using Xunit;

namespace HtmxMvc.Infrastructure.Tests;

public class DatabaseSeederTests
{
    [Fact]
    public async Task Seed_inserts_5_contacts_into_empty_database()
    {
        await using var ctx = new SqliteTestContext();

        DatabaseSeeder.Seed(ctx.Db);

        Assert.Equal(5, ctx.Db.Contacts.Count());
    }

    [Fact]
    public async Task Seed_is_idempotent()
    {
        await using var ctx = new SqliteTestContext();

        DatabaseSeeder.Seed(ctx.Db);
        DatabaseSeeder.Seed(ctx.Db);
        DatabaseSeeder.Seed(ctx.Db);

        Assert.Equal(5, ctx.Db.Contacts.Count());
    }
}
