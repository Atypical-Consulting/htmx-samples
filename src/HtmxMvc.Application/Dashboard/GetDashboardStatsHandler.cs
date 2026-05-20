using HtmxMvc.Domain;

namespace HtmxMvc.Application.Dashboard;

public sealed class GetDashboardStatsHandler(IContactRepository repo)
{
    public async Task<DashboardStats> ExecuteAsync(int recent, CancellationToken ct = default)
    {
        var clampedRecent = Math.Clamp(recent, 1, 50);
        var all = await repo.GetAllAsync(ct);
        var recentContacts = all
            .OrderByDescending(c => c.Id)
            .Take(clampedRecent)
            .ToList();
        return new DashboardStats(all.Count, clampedRecent, recentContacts);
    }
}
