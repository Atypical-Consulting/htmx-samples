using Microsoft.AspNetCore.Mvc.Razor;

namespace HtmxMvc.Features;

public sealed class FeatureViewLocationExpander : IViewLocationExpander
{
    private static readonly string[] FeatureLocations =
    [
        "/Features/{1}/Views/{0}.cshtml",
        "/Features/Shared/{0}.cshtml",
    ];

    public IEnumerable<string> ExpandViewLocations(
        ViewLocationExpanderContext context,
        IEnumerable<string> viewLocations)
        => FeatureLocations.Concat(viewLocations);

    public void PopulateValues(ViewLocationExpanderContext context) { }
}
