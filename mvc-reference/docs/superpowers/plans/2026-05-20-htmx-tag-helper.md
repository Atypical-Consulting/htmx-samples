# HTMX `hx-action` Tag Helper Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a single MVC tag helper that turns `hx-action="Edit" hx-route-id="@Model.Id"` into the correct `hx-get="/contacts/1/edit"` by reading controller action metadata, then refactor all four views to use it. Catches view/route mismatches at render time instead of at HTMX click time.

**Architecture:** One TagHelper class in `HtmxMvc.Web/TagHelpers/HtmxActionTagHelper.cs`. Reads `IActionDescriptorCollectionProvider` to find the matching action and its `HttpMethodMetadata`, uses `LinkGenerator` to produce the URL, emits `hx-{verb}="{url}"` on the output. No changes to Domain / Application / Infrastructure / Controllers.

**Tech Stack:** ASP.NET Core MVC on .NET 10. No new packages.

**Spec:** `docs/superpowers/specs/2026-05-20-htmx-tag-helper-design.md`

**Testing approach:** No unit tests for the tag helper itself (would need a full MVC test harness for one ~80-line class). Verification is the existing curl-based CRUD smoke test plus a targeted assertion that the rendered HTML contains exactly the expected `hx-get="/contacts/1/edit"` etc.

---

## File Structure

```
src/HtmxMvc.Web/
  TagHelpers/
    HtmxActionTagHelper.cs                  [Task 1, NEW]
  Views/
    _ViewImports.cshtml                     [Task 1, MODIFY — add @addTagHelper]
    Shared/
      _ContactRow.cshtml                    [Task 2, MODIFY — Edit/Delete buttons]
      _ContactEditRow.cshtml                [Task 2, MODIFY — form + Cancel]
    Contacts/
      Index.cshtml                          [Task 2, MODIFY — search input + add form]
README.md                                   [Task 3, MODIFY — mention tag helper]
```

No new tests, no new packages, no new projects.

---

## Task 1: Implement and register the tag helper

**Files:**
- Create: `src/HtmxMvc.Web/TagHelpers/HtmxActionTagHelper.cs`
- Modify: `src/HtmxMvc.Web/Views/_ViewImports.cshtml`

- [ ] **Step 1: Write the tag helper**

Create `src/HtmxMvc.Web/TagHelpers/HtmxActionTagHelper.cs`:
```csharp
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
```

- [ ] **Step 2: Register the tag helper assembly in _ViewImports**

Add one line to `src/HtmxMvc.Web/Views/_ViewImports.cshtml`. Current content:
```cshtml
@using HtmxMvc
@using HtmxMvc.Domain
@using HtmxMvc.Application.Contacts
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

Add a line so it becomes:
```cshtml
@using HtmxMvc
@using HtmxMvc.Domain
@using HtmxMvc.Application.Contacts
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@addTagHelper *, HtmxMvc.Web
```

The second arg is the assembly name (`HtmxMvc.Web.csproj` produces `HtmxMvc.Web.dll`), not the namespace.

- [ ] **Step 3: Verify the project builds**

Run: `dotnet build src/HtmxMvc.Web`
Expected: `Build succeeded` with 0 errors.

The views still use the old `hx-get` / `hx-post` etc. attributes, so nothing has changed visibly yet — but the tag helper is registered and ready.

- [ ] **Step 4: Commit**

```powershell
git add src/HtmxMvc.Web/TagHelpers src/HtmxMvc.Web/Views/_ViewImports.cshtml
git commit -m "$(@'
feat: add HtmxActionTagHelper that emits hx-{verb} from action metadata

Reads IActionDescriptorCollectionProvider to find the matching
controller action, extracts its HttpMethodMetadata, then uses
LinkGenerator to produce the URL. Emits hx-get/hx-post/hx-put/hx-delete
on the output element and strips hx-action/hx-controller/hx-route-*.

Throws InvalidOperationException at render time for unknown actions,
missing HTTP methods, or unresolvable URLs so dev sees failures
immediately as a 500 response.

Co-Authored-By: Claude Opus 4.7 (1M context) <noreply@anthropic.com>
'@)"
```

---

## Task 2: Refactor the four views to use `hx-action`

**Files:**
- Modify: `src/HtmxMvc.Web/Views/Shared/_ContactRow.cshtml`
- Modify: `src/HtmxMvc.Web/Views/Shared/_ContactEditRow.cshtml`
- Modify: `src/HtmxMvc.Web/Views/Contacts/Index.cshtml`

- [ ] **Step 1: Refactor `_ContactRow.cshtml` Edit button**

In `src/HtmxMvc.Web/Views/Shared/_ContactRow.cshtml`, replace:
```cshtml
        <button class="text-blue-600 hover:underline"
                hx-get="/contacts/@Model.Id/edit"
                hx-target="#contact-@Model.Id"
                hx-swap="outerHTML">Edit</button>
```
with:
```cshtml
        <button class="text-blue-600 hover:underline"
                hx-action="Edit"
                hx-route-id="@Model.Id"
                hx-target="#contact-@Model.Id"
                hx-swap="outerHTML">Edit</button>
```

- [ ] **Step 2: Refactor `_ContactRow.cshtml` Delete button**

In the same file, replace:
```cshtml
        <button class="text-red-600 hover:underline"
                hx-delete="/contacts/@Model.Id"
                hx-target="#contact-@Model.Id"
                hx-swap="outerHTML"
                hx-confirm="Delete @Model.Name?">Delete</button>
```
with:
```cshtml
        <button class="text-red-600 hover:underline"
                hx-action="Delete"
                hx-route-id="@Model.Id"
                hx-target="#contact-@Model.Id"
                hx-swap="outerHTML"
                hx-confirm="Delete @Model.Name?">Delete</button>
```

- [ ] **Step 3: Refactor `_ContactEditRow.cshtml` form opening tag**

In `src/HtmxMvc.Web/Views/Shared/_ContactEditRow.cshtml`, replace:
```cshtml
        <form hx-put="/contacts/@Model.Id"
              hx-target="#contact-@Model.Id"
              hx-swap="outerHTML"
              class="flex flex-wrap gap-2 items-center">
```
with:
```cshtml
        <form hx-action="Update"
              hx-route-id="@Model.Id"
              hx-target="#contact-@Model.Id"
              hx-swap="outerHTML"
              class="flex flex-wrap gap-2 items-center">
```

- [ ] **Step 4: Refactor `_ContactEditRow.cshtml` Cancel button**

In the same file, replace:
```cshtml
            <button type="button"
                    hx-get="/contacts/@Model.Id"
                    hx-target="#contact-@Model.Id"
                    hx-swap="outerHTML"
                    class="bg-slate-200 hover:bg-slate-300 rounded px-3 py-1">Cancel</button>
```
with:
```cshtml
            <button type="button"
                    hx-action="Row"
                    hx-route-id="@Model.Id"
                    hx-target="#contact-@Model.Id"
                    hx-swap="outerHTML"
                    class="bg-slate-200 hover:bg-slate-300 rounded px-3 py-1">Cancel</button>
```

- [ ] **Step 5: Refactor `Index.cshtml` search input**

In `src/HtmxMvc.Web/Views/Contacts/Index.cshtml`, replace:
```cshtml
        <input id="q" type="search" name="q"
               placeholder="Type to filter..."
               class="w-full border border-slate-300 rounded px-3 py-2"
               hx-get="/contacts/list"
               hx-trigger="keyup changed delay:300ms, search"
               hx-target="#contact-rows"
               hx-swap="innerHTML" />
```
with:
```cshtml
        <input id="q" type="search" name="q"
               placeholder="Type to filter..."
               class="w-full border border-slate-300 rounded px-3 py-2"
               hx-action="List"
               hx-trigger="keyup changed delay:300ms, search"
               hx-target="#contact-rows"
               hx-swap="innerHTML" />
```

- [ ] **Step 6: Refactor `Index.cshtml` add form**

In the same file, replace:
```cshtml
    <form hx-post="/contacts"
          hx-target="#contact-rows"
          hx-swap="afterbegin"
          hx-on::after-request="if(event.detail.successful) this.reset()"
          class="flex flex-wrap gap-2 items-end bg-white p-4 border border-slate-200 rounded">
```
with:
```cshtml
    <form hx-action="Create"
          hx-target="#contact-rows"
          hx-swap="afterbegin"
          hx-on::after-request="if(event.detail.successful) this.reset()"
          class="flex flex-wrap gap-2 items-end bg-white p-4 border border-slate-200 rounded">
```

- [ ] **Step 7: Build to verify Razor still compiles**

Run: `dotnet build`
Expected: `Build succeeded`, 0 errors.

- [ ] **Step 8: Run the app and verify the tag helper produced the right URLs**

Start the app in the background:
```powershell
dotnet run --project src/HtmxMvc.Web --urls http://localhost:5099
```

Wait for `Now listening on: http://localhost:5099`, then verify each rendered URL exactly. From bash:
```bash
curl -s http://localhost:5099/ -o /tmp/idx.html

echo "--- search input ---"
grep -F 'hx-get="/contacts/list"' /tmp/idx.html && echo "ok" || echo "FAIL"
grep -F 'hx-action' /tmp/idx.html && echo "FAIL: hx-action leaked" || echo "ok: stripped"

echo "--- add form ---"
grep -F 'hx-post="/contacts"' /tmp/idx.html && echo "ok" || echo "FAIL"

echo "--- Ada row Edit button ---"
grep -F 'hx-get="/contacts/1/edit"' /tmp/idx.html && echo "ok" || echo "FAIL"

echo "--- Ada row Delete button ---"
grep -F 'hx-delete="/contacts/1"' /tmp/idx.html && echo "ok" || echo "FAIL"

echo "--- hx-route-id stripped ---"
grep -F 'hx-route-id' /tmp/idx.html && echo "FAIL: hx-route-id leaked" || echo "ok: stripped"

echo "--- edit-row form (Update) ---"
curl -s http://localhost:5099/contacts/1/edit -o /tmp/edit.html
grep -F 'hx-put="/contacts/1"' /tmp/edit.html && echo "ok" || echo "FAIL"

echo "--- edit-row Cancel button ---"
grep -F 'hx-get="/contacts/1"' /tmp/edit.html && echo "ok" || echo "FAIL"
```

Expected: every line ends in `ok`. No `FAIL`. No `hx-action` or `hx-route-id` strings in the rendered output.

- [ ] **Step 9: Full CRUD smoke test (same as previous tasks)**

```bash
JAR=/tmp/tagh-cookies.txt
rm -f $JAR
curl -sc $JAR http://localhost:5099/ -o /tmp/idx2.html
TOKEN=$(grep -oE 'RequestVerificationToken&quot;: &quot;[A-Za-z0-9_\-]+' /tmp/idx2.html | head -1 | sed 's/.*&quot;//')

echo "POST:"
curl -sb $JAR -X POST "http://localhost:5099/contacts" -H "RequestVerificationToken: $TOKEN" \
  --data-urlencode "Name=Tag Helper Test" --data-urlencode "Email=t@h.test" --data-urlencode "Phone=000" \
  -o /tmp/new.html -w "status=%{http_code}\n"
NEWID=$(grep -oE 'id="contact-[0-9]+"' /tmp/new.html | head -1 | grep -oE '[0-9]+')
echo "new id: $NEWID"

echo "Verify new row has tag-helper-generated URLs:"
grep -F "hx-get=\"/contacts/$NEWID/edit\"" /tmp/new.html && echo "ok: Edit URL"
grep -F "hx-delete=\"/contacts/$NEWID\"" /tmp/new.html && echo "ok: Delete URL"

echo "DELETE:"
curl -sb $JAR -X DELETE "http://localhost:5099/contacts/$NEWID" -H "RequestVerificationToken: $TOKEN" \
  -w "status=%{http_code}\n" -o /dev/null
```

Expected: POST 200 with new id, both URL greps `ok`, DELETE 200.

- [ ] **Step 10: Stop the app and commit**

Stop the dotnet run background task, then:
```powershell
git add src/HtmxMvc.Web/Views
git commit -m "$(@'
refactor: use hx-action in all views instead of magic-string URLs

Seven hx-get / hx-post / hx-put / hx-delete URL strings replaced by
hx-action="..." with route values via hx-route-*. The tag helper
generates the actual URL from controller metadata at render time, so
renaming an action or changing a route attribute now shows up as a
500 at page load instead of as a silent client-side 404 on click.

Co-Authored-By: Claude Opus 4.7 (1M context) <noreply@anthropic.com>
'@)"
```

---

## Task 3: README — document the tag helper

**Files:**
- Modify: `README.md`

- [ ] **Step 1: Add a section about the tag helper**

In `README.md`, find the `## Notes` section. Insert a new section *before* it:

```markdown
## Routing via tag helper

Views write `<button hx-action="Edit" hx-route-id="@Model.Id" ...>` instead of
`<button hx-get="/contacts/1/edit" ...>`. The `HtmxActionTagHelper` resolves
the action to its URL via `LinkGenerator` and emits the correct `hx-{verb}`
based on the action's `[HttpGet]` / `[HttpPost]` / `[HttpPut]` / `[HttpDelete]`
attribute.

Effect: renaming an action or changing a route attribute fails at render time
(500 with a descriptive message) instead of silently breaking HTMX requests
in the browser. See `src/HtmxMvc.Web/TagHelpers/HtmxActionTagHelper.cs`.
```

- [ ] **Step 2: Commit**

```powershell
git add README.md
git commit -m "$(@'
docs: document the hx-action tag helper in README

Co-Authored-By: Claude Opus 4.7 (1M context) <noreply@anthropic.com>
'@)"
```

---

## Done

After Task 3, the views express their routes as typed action references and the build verifies them at render time. Re-running the existing browser MCP test sequence should produce identical visible behavior with different rendered HTML.

```powershell
dotnet test
dotnet run --project src/HtmxMvc.Web
```
