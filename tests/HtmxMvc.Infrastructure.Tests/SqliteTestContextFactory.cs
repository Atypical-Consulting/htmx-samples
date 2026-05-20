using HtmxMvc.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HtmxMvc.Infrastructure.Tests;

internal sealed class SqliteTestContext : IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    public AppDbContext Db { get; }

    public SqliteTestContext()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;
        Db = new AppDbContext(options);
        Db.Database.EnsureCreated();
    }

    public async ValueTask DisposeAsync()
    {
        await Db.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
