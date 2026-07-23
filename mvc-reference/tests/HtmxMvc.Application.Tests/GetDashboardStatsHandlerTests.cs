using HtmxMvc.Application.Dashboard;
using HtmxMvc.Domain;
using Xunit;

namespace HtmxMvc.Application.Tests;

public class GetDashboardStatsHandlerTests
{
    private static FakeContactRepository RepoWith(int count)
    {
        var seed = Enumerable.Range(1, count)
            .Select(i => new Contact { Name = $"C{i}", Email = $"c{i}@x", Phone = "" })
            .ToArray();
        return new FakeContactRepository(seed);
    }

    [Fact]
    public async Task Reports_total_count()
    {
        var handler = new GetDashboardStatsHandler(RepoWith(12));

        var stats = await handler.ExecuteAsync(recent: 5);

        Assert.Equal(12, stats.TotalContacts);
    }

    [Fact]
    public async Task Returns_recent_contacts_newest_first()
    {
        var handler = new GetDashboardStatsHandler(RepoWith(6));

        var stats = await handler.ExecuteAsync(recent: 3);

        Assert.Equal(3, stats.RecentCount);
        Assert.Equal(new[] { 6, 5, 4 }, stats.RecentContacts.Select(c => c.Id));
    }

    [Fact]
    public async Task Returns_fewer_than_requested_when_repo_is_smaller()
    {
        var handler = new GetDashboardStatsHandler(RepoWith(2));

        var stats = await handler.ExecuteAsync(recent: 10);

        Assert.Equal(2, stats.TotalContacts);
        Assert.Equal(2, stats.RecentContacts.Count);
    }

    [Fact]
    public async Task Clamps_recent_to_min_1()
    {
        var handler = new GetDashboardStatsHandler(RepoWith(5));

        var stats = await handler.ExecuteAsync(recent: 0);

        Assert.Equal(1, stats.RecentCount);
        Assert.Single(stats.RecentContacts);
    }

    [Fact]
    public async Task Clamps_recent_to_max_50()
    {
        var handler = new GetDashboardStatsHandler(RepoWith(60));

        var stats = await handler.ExecuteAsync(recent: 9999);

        Assert.Equal(50, stats.RecentCount);
        Assert.Equal(50, stats.RecentContacts.Count);
    }

    [Fact]
    public async Task Returns_empty_recent_list_when_repo_is_empty()
    {
        var handler = new GetDashboardStatsHandler(RepoWith(0));

        var stats = await handler.ExecuteAsync(recent: 5);

        Assert.Equal(0, stats.TotalContacts);
        Assert.Empty(stats.RecentContacts);
    }
}
