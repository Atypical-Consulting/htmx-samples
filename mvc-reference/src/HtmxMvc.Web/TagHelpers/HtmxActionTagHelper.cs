using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;

namespace HtmxMvc.TagHelpers;

[HtmlTargetElement("*", Attributes = ActionAttributeName)]
public sealed class HtmxActionTagHelper : TagHelper
{
    private const string ActionAttributeName = "hx-action";
    private const string ControllerAttributeName = "hx-controller";
    private const string RouteAttributePrefix = "hx-route-";

    private readonly LinkGenerator _linkGenerator;
    private readonly IActionDescriptorCollectionProvider _actions;

    public HtmxActionTagHelper(
        LinkGenerator linkGenerator,
        IActionDescriptorCollectionProvider actions)
    {
        _linkGenerator = linkGenerator;
        _actions = actions;
    }

    [ViewContext, HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    [HtmlAttributeName(ActionAttributeName)]
    public string? Action { get; set; }

    [HtmlAttributeName(ControllerAttributeName)]
    public string? Controller { get; set; }

    [HtmlAttributeName("", DictionaryAttributePrefix = RouteAttributePrefix)]
    public IDictionary<string, string?> RouteValues { get; set; }
        = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (string.IsNullOrEmpty(Action)) return;

        var controller = Controller
            ?? ViewContext.RouteData.Values["controller"]?.ToString()
            ?? throw new InvalidOperationException(
                "hx-action requires hx-controller when no current controller is set.");

        var descriptor = _actions.ActionDescriptors.Items
            .OfType<ControllerActionDescriptor>()
            .FirstOrDefault(d =>
                d.ControllerName.Equals(controller, StringComparison.OrdinalIgnoreCase) &&
                d.ActionName.Equals(Action, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException(
                $"No action '{Action}' on controller '{controller}'.");

        var httpMethods = descriptor.EndpointMetadata
            .OfType<HttpMethodMetadata>()
            .SelectMany(m => m.HttpMethods)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (httpMethods.Count != 1)
        {
            throw new InvalidOperationException(
                $"Action '{controller}.{Action}' must declare exactly one HTTP method (found {httpMethods.Count}).");
        }

        var url = _linkGenerator.GetPathByAction(
            httpContext: ViewContext.HttpContext,
            action: Action,
            controller: controller,
            values: new RouteValueDictionary(RouteValues))
            ?? throw new InvalidOperationException(
                $"Could not generate URL for action '{controller}.{Action}'. Check hx-route-* values.");

        output.Attributes.SetAttribute($"hx-{httpMethods[0].ToLowerInvariant()}", url);
        output.Attributes.RemoveAll(ActionAttributeName);
        output.Attributes.RemoveAll(ControllerAttributeName);
        foreach (var key in RouteValues.Keys)
        {
            output.Attributes.RemoveAll($"{RouteAttributePrefix}{key}");
        }
    }
}
