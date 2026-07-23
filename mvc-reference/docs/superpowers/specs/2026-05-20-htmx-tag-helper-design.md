# HTMX `hx-action` Tag Helper — Design

**Date:** 2026-05-20
**Stack:** ASP.NET Core MVC on .NET 10 (existing). New code lives only in `HtmxMvc.Web`.

## Goal

Replace the seven magic-string URLs in the views (`hx-get="/contacts/@Model.Id/edit"` etc.) with typed action references that the server resolves via `LinkGenerator` at render time. Catches typos and route refactors at render time instead of letting them silently break HTMX requests in the browser.

## Non-goals

- Roslyn analyzer for invalid `hx-action` values at build time (separate, much bigger project)
- `nameof(ContactsController.Edit)` support — `nameof` returns `"Edit"` already, but handling `Async` suffix stripping is more complexity than the demo needs
- Tag helpers for non-routing HTMX attributes (`hx-target`, `hx-swap`, `hx-confirm`, etc.) — they already work as plain attributes
- Changes to Domain / Application / Infrastructure / Controllers
- Antiforgery wiring (already handled via `<body hx-headers="…">` in `_Layout.cshtml`)
- Cross-controller calls beyond what `hx-controller="…"` already enables
- Unit tests for the tag helper (see Testing section)

## Component

A single tag helper class:

```
src/HtmxMvc.Web/TagHelpers/HtmxActionTagHelper.cs
```

Namespace: `HtmxMvc.TagHelpers` (matches `<RootNamespace>HtmxMvc</RootNamespace>` on the Web project).

Registered globally via one new line in `src/HtmxMvc.Web/Views/_ViewImports.cshtml`:

```cshtml
@addTagHelper *, HtmxMvc.Web
```

## API surface

```cshtml
<button hx-action="Edit"
        hx-route-id="@Model.Id"
        hx-target="#contact-@Model.Id"
        hx-swap="outerHTML">Edit</button>
```

Produces:

```html
<button hx-get="/contacts/1/edit"
        hx-target="#contact-1"
        hx-swap="outerHTML">Edit</button>
```

### Attributes the helper consumes

| Attribute | Required? | Meaning |
|---|---|---|
| `hx-action` | yes | Controller action name, e.g. `"Edit"`. Triggers the tag helper. |
| `hx-controller` | no | Controller name. Defaults to `ViewContext.RouteData.Values["controller"]` (i.e. the current request's controller). |
| `hx-route-*` | no | Route value dictionary, mirroring ASP.NET's built-in `asp-route-*` convention. Example: `hx-route-id="@Model.Id"`. |

The helper attaches to `[HtmlTargetElement("*", Attributes = "hx-action")]` — any element with `hx-action`. Matches HTMX's own "works on anything" ethos.

### Attributes the helper emits

- `hx-{verb}="{url}"` (one of `hx-get`, `hx-post`, `hx-put`, `hx-delete`), determined by reading the action's `HttpMethodMetadata`.

### Attributes the helper strips from the output

- `hx-action`
- `hx-controller` (if present)
- All `hx-route-*` attributes

### Attributes that pass through untouched

Everything else: `hx-target`, `hx-swap`, `hx-confirm`, `hx-trigger`, `hx-on:*`, `class`, `id`, etc.

## Resolution algorithm

1. If `hx-action` is null/empty, return without modification.
2. Resolve `controller` = `Controller` property if set, else `ViewContext.RouteData.Values["controller"]`. If neither is present, throw.
3. Find the matching `ControllerActionDescriptor` via `IActionDescriptorCollectionProvider.ActionDescriptors.Items`:
   - Match `ControllerName` and `ActionName` case-insensitively.
4. From the matched descriptor's `EndpointMetadata`, read the single `HttpMethodMetadata`:
   - Zero HTTP methods → throw
   - More than one HTTP method → throw (the helper can only emit one `hx-{verb}`)
5. Build the URL via `LinkGenerator.GetPathByAction(ViewContext.HttpContext, action, controller, new RouteValueDictionary(RouteValues))`.
   - Null result → throw (likely missing a required route value)
6. Set `hx-{verb.ToLowerInvariant()}="{url}"` on the output.
7. Remove `hx-action`, `hx-controller`, and every `hx-route-*` attribute from the output.

## Failure modes

All checks throw `InvalidOperationException` with a descriptive message at render time. These surface immediately during development (the page returns a 500 with the message), so there's no silent failure path.

| Failure | Message (substring) |
|---|---|
| No controller (neither property nor RouteData) | `hx-action requires hx-controller when no current controller is set` |
| Action not found | `No action '{Action}' on controller '{Controller}'` |
| Zero or multiple HTTP methods | `Action '{Controller}.{Action}' must declare exactly one HTTP method (found N)` |
| URL generation failed | `Could not generate URL for action '{Controller}.{Action}'. Check hx-route-* values.` |

## Constructor dependencies

The tag helper takes two services via constructor injection:

- `LinkGenerator` — built into the ASP.NET routing pipeline; no additional registration needed.
- `IActionDescriptorCollectionProvider` — also built in.

Both are registered by `AddControllersWithViews()`, which `Program.cs` already calls. No changes to DI.

## Views to refactor

After the tag helper compiles, four views change. Only the URL-bearing `hx-*` attributes are touched; everything else (Tailwind classes, `hx-target`, `hx-swap`, `hx-confirm`, `hx-trigger`) stays as-is.

### `_ContactRow.cshtml`
- Edit `<button>`: `hx-get="/contacts/@Model.Id/edit"` → `hx-action="Edit" hx-route-id="@Model.Id"`
- Delete `<button>`: `hx-delete="/contacts/@Model.Id"` → `hx-action="Delete" hx-route-id="@Model.Id"`

### `_ContactEditRow.cshtml`
- `<form>`: `hx-put="/contacts/@Model.Id"` → `hx-action="Update" hx-route-id="@Model.Id"`
- Cancel `<button>`: `hx-get="/contacts/@Model.Id"` → `hx-action="Row" hx-route-id="@Model.Id"`

### `Contacts/Index.cshtml`
- Search `<input>`: `hx-get="/contacts/list"` → `hx-action="List"`
- Add `<form>`: `hx-post="/contacts"` → `hx-action="Create"`

Seven URL strings replaced by typed action references.

## Behavioral parity

The browser test sequence from `docs/superpowers/screenshots/` must still pass. Verification is the same curl-based CRUD smoke test as before:

1. `GET /` returns the seeded list (Ada, Alan, Grace, Edsger, Margaret).
2. `GET /contacts/list?q=ada` returns just Ada.
3. `POST /contacts` (with antiforgery) creates contact 6.
4. `PUT /contacts/6` updates it.
5. `DELETE /contacts/6` removes it.

The HTML inspection check tightens: verify the rendered Edit button's `hx-get` attribute equals exactly `/contacts/1/edit` (was a magic string in the .cshtml; should now be `LinkGenerator`-produced). This proves the tag helper actually ran.

## Testing

No unit tests for the tag helper itself. Testing tag helpers in isolation requires reconstructing a `TagHelperContext` + `TagHelperOutput` + mocking `LinkGenerator` and `IActionDescriptorCollectionProvider` — a lot of ceremony for one ~80-line class. The curl smoke test exercises the helper end-to-end and the rendered HTML check confirms the resolved URL.

If future work wants real coverage, the standard ASP.NET pattern is a `WebApplicationFactory<Program>` test that spins up the app and scrapes the rendered HTML — that belongs in a future `HtmxMvc.Web.Tests` project, out of scope here.

## Open items resolved during plan-phase

- Whether the tag helper namespace should be `HtmxMvc.TagHelpers` or `HtmxMvc.Web.TagHelpers` (recommend the former since `<RootNamespace>HtmxMvc</RootNamespace>` is what the rest of the project uses)
- Exact wording of the `@addTagHelper` directive in `_ViewImports.cshtml` (recommended: `@addTagHelper *, HtmxMvc.Web` — assembly name, not namespace)
