# HTMX Contacts Demo

Small ASP.NET Core MVC (.NET 10) demo showing six HTMX 2 patterns in one
cohesive page: active search, append-on-create, inline edit,
swap-back-on-cancel, in-place update, and delete-row. Data lives in an
in-memory singleton. Styling via Tailwind Play CDN.

## Run

```powershell
dotnet run --project src/HtmxMvc
```

Then open the URL printed by Kestrel (or pass `--urls http://localhost:5099`).

## Try it

- Type in the search box — debounced active search (`hx-trigger="keyup changed delay:300ms"`)
- Add a contact via the form — new row prepended (`hx-swap="afterbegin"`)
- Click **Edit** on a row — inline edit form replaces the row (`hx-swap="outerHTML"`)
- Click **Save** — row swaps back to read-only with new values
- Click **Cancel** — row swaps back to the unedited read-only row
- Click **Delete** — row removed from the table (`hx-confirm` prompts first)

## Layout

| Path | Responsibility |
|---|---|
| `src/HtmxMvc/Models/Contact.cs` | The model |
| `src/HtmxMvc/Services/ContactService.cs` | Thread-safe in-memory store, seeded |
| `src/HtmxMvc/Controllers/ContactsController.cs` | One page action + six partial actions |
| `src/HtmxMvc/Views/Contacts/Index.cshtml` | The page (search, add form, table) |
| `src/HtmxMvc/Views/Shared/_Layout.cshtml` | HTMX + Tailwind CDN, antiforgery `hx-headers` |
| `src/HtmxMvc/Views/Shared/_ContactRow.cshtml` | Read-only `<tr>` |
| `src/HtmxMvc/Views/Shared/_ContactEditRow.cshtml` | Inline edit `<tr>` |
| `src/HtmxMvc/Views/Shared/_ContactList.cshtml` | `<tbody>` rows for search results |

## Notes

- Antiforgery: `[AutoValidateAntiforgeryToken]` on the controller; the token is
  injected into every HTMX request via the `hx-headers` attribute on `<body>`
  in `_Layout.cshtml`.
- No client-side JavaScript beyond HTMX itself.
- Pinning: HTMX is loaded from `unpkg.com` and Tailwind from `cdn.tailwindcss.com`.
  Both are CDN references suitable for a demo only — for production, vendor
  them locally and pin a Subresource Integrity hash.

## Design docs

- Spec: `docs/superpowers/specs/2026-05-19-htmx-contacts-demo-design.md`
- Plan: `docs/superpowers/plans/2026-05-19-htmx-contacts-demo.md`
