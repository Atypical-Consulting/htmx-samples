# Hexagonal Refactor — Design

**Date:** 2026-05-20
**Stack:** ASP.NET Core MVC on .NET 10, HTMX 2, Tailwind (CDN), xUnit.v3
**Refactors:** the HTMX Contacts Demo from a single project into four projects + two test projects.

## Goal

Restructure the demo to demonstrate hexagonal (ports & adapters) architecture without changing any user-facing behavior. The browser test in `docs/superpowers/screenshots/` should still pass after the refactor.

## Non-goals

- Changing routes, views, HTMX wiring, or anything visible to the browser
- Real persistence — the in-memory store stays
- MediatR or any CQRS framework — handler-per-use-case but no library
- Domain richness — `Contact` stays anemic; no value objects, no `Rename()` methods
- Result types (`Result<T, E>`) — `null`/`bool` returns as before
- Authentication, authorization
- Behavior-changing performance work (the in-memory search is fine)

## Architectural shape

```
HtmxMvc.slnx
src/
  HtmxMvc.Domain/             no project refs        — entity + port
  HtmxMvc.Application/        → Domain               — use cases (handlers)
  HtmxMvc.Infrastructure/     → Domain               — port implementation
  HtmxMvc.Web/                → Application, Infra   — controllers, views (composition root)
tests/
  HtmxMvc.Application.Tests/  → Application, Domain  — handler tests with fake repo
```

**Dependency direction:** all arrows point inward toward `Domain`. `Infrastructure` never references `Application`. `Web` is the composition root: it sees both `Application` (to call handlers) and `Infrastructure` (to register the in-memory adapter in `Program.cs`).

Domain has no tests project — the entity is anemic and has no logic worth testing. If domain rules accumulate later, add `HtmxMvc.Domain.Tests`.

## Layers

### `HtmxMvc.Domain`

```
Contact.cs                  entity (Id, Name, Email, Phone)
IContactRepository.cs       output port — pure CRUD
```

```csharp
public sealed class Contact
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
}

public interface IContactRepository
{
    Task<IReadOnlyList<Contact>> GetAllAsync(CancellationToken ct = default);
    Task<Contact?> GetAsync(int id, CancellationToken ct = default);
    Task<Contact> AddAsync(Contact contact, CancellationToken ct = default);
    Task<Contact?> UpdateAsync(int id, Contact contact, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
```

No `SearchAsync` on the port — search is application logic (see below).

No data annotations on `Contact`. Validation is presentation concern; lives on `ContactInput` in Application.

### `HtmxMvc.Application`

```
Contacts/
  ContactInput.cs                 record with [Required] etc.
  ListContactsHandler.cs
  SearchContactsHandler.cs        owns the search predicate
  GetContactHandler.cs
  AddContactHandler.cs
  UpdateContactHandler.cs
  DeleteContactHandler.cs
DependencyInjection.cs            AddApplication() extension
```

```csharp
public sealed record ContactInput
{
    [Required, StringLength(100)] public string Name { get; init; } = "";
    [StringLength(200)]            public string Email { get; init; } = "";
    [StringLength(50)]             public string Phone { get; init; } = "";
}
```

Handler convention — one class per use case, primary constructor for dependencies, single `ExecuteAsync` method:

```csharp
public sealed class SearchContactsHandler(IContactRepository repo)
{
    public async Task<IReadOnlyList<Contact>> ExecuteAsync(string? q, CancellationToken ct = default)
    {
        var all = await repo.GetAllAsync(ct);
        if (string.IsNullOrWhiteSpace(q)) return all;
        return all.Where(c =>
            c.Name.Contains(q,  StringComparison.OrdinalIgnoreCase) ||
            c.Email.Contains(q, StringComparison.OrdinalIgnoreCase) ||
            c.Phone.Contains(q, StringComparison.OrdinalIgnoreCase))
        .ToList();
    }
}
```

`AddContactHandler` and `UpdateContactHandler` take `ContactInput`; map to a `Contact` instance before calling the repository.

`DependencyInjection.AddApplication()` registers all six handlers as `Scoped`.

### `HtmxMvc.Infrastructure`

```
Contacts/
  InMemoryContactRepository.cs       singleton, thread-safe via Lock
DependencyInjection.cs               AddInfrastructure() extension
```

Renamed-and-moved version of today's `ContactService`. Now implements `IContactRepository` and returns `Task.FromResult(...)` for everything. The seeded data lives in the constructor.

`AddInfrastructure()` registers `IContactRepository → InMemoryContactRepository` as `Singleton`.

### `HtmxMvc.Web`

Web project renamed from `HtmxMvc` to `HtmxMvc.Web`. No namespace changes for views (they don't reference `HtmxMvc.*` types beyond `Contact`).

`ContactsController` constructor now takes the six handlers via primary constructor:

```csharp
[AutoValidateAntiforgeryToken]
public sealed class ContactsController(
    ListContactsHandler list,
    SearchContactsHandler search,
    GetContactHandler get,
    AddContactHandler add,
    UpdateContactHandler update,
    DeleteContactHandler delete) : Controller
{
    [HttpGet("/")]
    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await list.ExecuteAsync(ct));

    // ... etc. All actions become async, take CancellationToken,
    //     and bind to ContactInput where they currently bind to Contact.
}
```

`Program.cs`:
```csharp
builder.Services.AddControllersWithViews();
builder.Services.AddApplication();
builder.Services.AddInfrastructure();
```

Views are unchanged — they still bind to `IReadOnlyList<Contact>` (the domain entity is what they render).

### `tests/HtmxMvc.Application.Tests`

xUnit.v3 (`xunit.v3`, `xunit.v3.runner.visualstudio`, `Microsoft.NET.Test.Sdk`).

```
FakeContactRepository.cs           inline test double, in-memory, deterministic
ListContactsHandlerTests.cs
SearchContactsHandlerTests.cs      the meaty one — case-insensitive contains, empty/null q
AddContactHandlerTests.cs
UpdateContactHandlerTests.cs       not-found returns null
DeleteContactHandlerTests.cs       not-found returns false
```

The fake repo is hand-written (not Moq) — one ~30-line class, no framework dependency, and demonstrates that the application layer can be tested without `Infrastructure`. The tests reference `Application` (handlers) and `Domain` (Contact), but NOT `Infrastructure`. This is the load-bearing proof that hexagonal works.

`GetContactHandlerTests` is omitted — the handler is a 1-line pass-through and adds zero coverage value.

## Behavioral parity

The browser tests in `docs/superpowers/screenshots/` should still pass after the refactor. Specifically:

1. Initial load shows 5 seeded contacts (Ada/Alan/Grace/Edsger/Margaret).
2. Search `ada` returns only Ada.
3. POST a contact prepends it.
4. Edit/Save updates in place.
5. Cancel restores the original.
6. Delete removes the row.

Smoke test: `dotnet build && dotnet test && dotnet run --project src/HtmxMvc.Web`, then `curl /` returns 200 with all five seeded names.

## Open items resolved during plan-phase

- Exact xUnit.v3 package versions
- Whether `HtmxMvc.Web` keeps the existing `HtmxMvc` root namespace for views or moves to `HtmxMvc.Web` (recommend: keep `HtmxMvc` to avoid touching every `@model` and `@using`)
- Whether to register handlers as `Scoped` (default) or `Transient` (also fine for stateless handlers) — recommend `Scoped` because there's nothing to gain from `Transient` and `Scoped` is conventional
