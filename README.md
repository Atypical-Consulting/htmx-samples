# HTMX Contacts Demo

Small ASP.NET Core MVC (.NET 10) demo showing six HTMX 2 patterns in one
cohesive page: active search, append-on-create, inline edit,
swap-back-on-cancel, in-place update, and delete-row. Data lives in an
in-memory singleton. Styling via Tailwind Play CDN. Organized as a
hexagonal (ports & adapters) solution.

## Run

```powershell
dotnet run --project src/HtmxMvc.Web
```

Then open the URL printed by Kestrel (or pass `--urls http://localhost:5099`).

## Test

```powershell
dotnet test
```

Thirteen xUnit.v3 tests cover the Application handlers against a
hand-rolled fake repository — no Infrastructure reference, proving the
Application layer can be tested in isolation.

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
| `src/HtmxMvc.Domain/` | `Contact` entity and `IContactRepository` port. No dependencies. |
| `src/HtmxMvc.Application/` | One handler per use case (`ListContactsHandler`, `SearchContactsHandler`, etc.), `ContactInput` write DTO, `AddApplication()` DI extension. Depends only on Domain. |
| `src/HtmxMvc.Infrastructure/` | `InMemoryContactRepository` (thread-safe singleton, seeded), `AddInfrastructure()` DI extension. Depends only on Domain. |
| `src/HtmxMvc.Web/` | Composition root. `Program.cs` calls `AddApplication()` + `AddInfrastructure()`. `ContactsController` injects the six handlers. Views render `HtmxMvc.Domain.Contact`. |
| `tests/HtmxMvc.Application.Tests/` | xUnit.v3 tests against a hand-rolled `FakeContactRepository`. References `Application` + `Domain` only. |

**Project graph:** `Web` → `Application` + `Infrastructure`; `Application` and `Infrastructure` both → `Domain`. Domain has zero dependencies.

## Notes

- Antiforgery: `[AutoValidateAntiforgeryToken]` on the controller; the token is
  injected into every HTMX request via the `hx-headers` attribute on `<body>`
  in `_Layout.cshtml`.
- No client-side JavaScript beyond HTMX itself.
- Pinning: HTMX is loaded from `unpkg.com` and Tailwind from `cdn.tailwindcss.com`.
  Both are CDN references suitable for a demo only — for production, vendor
  them locally and pin a Subresource Integrity hash.

## Design docs

- Demo spec: `docs/superpowers/specs/2026-05-19-htmx-contacts-demo-design.md`
- Demo plan: `docs/superpowers/plans/2026-05-19-htmx-contacts-demo.md`
- Hex refactor spec: `docs/superpowers/specs/2026-05-20-hexagonal-refactor-design.md`
- Hex refactor plan: `docs/superpowers/plans/2026-05-20-hexagonal-refactor.md`
