using HtmxMvc.Application.Dashboard;
using HtmxMvc.Http;
using Microsoft.AspNetCore.Mvc;

namespace HtmxMvc.Features.Dashboard;

public sealed class DashboardController(GetDashboardStatsHandler stats) : Controller
{
    [HttpGet("/dashboard")]
    public async Task<IActionResult> Index(int recent = 5, CancellationToken ct = default)
    {
        var model = await stats.ExecuteAsync(recent, ct);
        if (!Request.IsHtmx())
        {
            return View(model);
        }
        Response.Htmx(h => h.PushUrl($"/dashboard?recent={model.RecentCount}"));
        return PartialView("_Stats", model);
    }
}
