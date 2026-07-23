using HtmxMvc.Domain;

namespace HtmxMvc.Application.Dashboard;

public sealed record DashboardStats(
    int TotalContacts,
    int RecentCount,
    IReadOnlyList<Contact> RecentContacts);
