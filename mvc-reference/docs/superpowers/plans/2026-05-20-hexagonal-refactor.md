# Hexagonal Refactor Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Restructure the HTMX Contacts Demo into Domain / Application / Infrastructure / Web projects plus a tests project, with zero user-facing behavior change.

**Architecture:** Hexagonal ports & adapters. `Domain` defines the `Contact` entity and `IContactRepository` port. `Application` has one handler per use case (ListContacts, SearchContacts, GetContact, AddContact, UpdateContact, DeleteContact), takes `ContactInput` records for write operations, and owns the search predicate. `Infrastructure` provides `InMemoryContactRepository`. `Web` is the composition root.

**Tech Stack:** .NET 10 multi-project solution, ASP.NET Core MVC + HTMX 2 (unchanged), xUnit.v3 for tests.

**Spec:** `docs/superpowers/specs/2026-05-20-hexagonal-refactor-design.md`

**Testing approach:** xUnit.v3 tests for Application handlers using a hand-rolled `FakeContactRepository`. No Infrastructure tests (the in-memory repo IS the implementation). Final verification: `dotnet build && dotnet test && curl /` smoke. Skip the full browser MCP replay unless something looks off — it was green on the previous build.

---

## File Structure

```
HtmxMvc.slnx                                            [Task 1, 4 — sln file edits]
src/
  HtmxMvc.Domain/
    HtmxMvc.Domain.csproj                               [Task 1]
    Contact.cs                                          [Task 1]
    IContactRepository.cs                               [Task 1]
  HtmxMvc.Infrastructure/
    HtmxMvc.Infrastructure.csproj                       [Task 2]
    Contacts/InMemoryContactRepository.cs               [Task 2]
    DependencyInjection.cs                              [Task 2]
  HtmxMvc.Application/
    HtmxMvc.Application.csproj                          [Task 3]
    Contacts/
      ContactInput.cs                                   [Task 3]
      ListContactsHandler.cs                            [Task 3]
      SearchContactsHandler.cs                          [Task 3]
      GetContactHandler.cs                              [Task 3]
      AddContactHandler.cs                              [Task 3]
      UpdateContactHandler.cs                           [Task 3]
      DeleteContactHandler.cs                           [Task 3]
    DependencyInjection.cs                              [Task 3]
  HtmxMvc.Web/                  (renamed from HtmxMvc/) [Task 4]
    HtmxMvc.Web.csproj          (renamed)               [Task 4]
    Program.cs                                          [Task 4]
    Controllers/ContactsController.cs                   [Task 4]
    Views/_ViewImports.cshtml                           [Task 4]
    Views/Shared/_ContactRow.cshtml                     [Task 4]
    Views/Shared/_ContactEditRow.cshtml                 [Task 4]
    Views/Shared/_ContactList.cshtml                    [Task 4]
    Views/Contacts/Index.cshtml                         [Task 4]
    Models/Contact.cs           [DELETED]               [Task 4]
    Services/ContactService.cs  [DELETED]               [Task 4]
tests/
  HtmxMvc.Application.Tests/
    HtmxMvc.Application.Tests.csproj                    [Task 5]
    FakeContactRepository.cs                            [Task 5]
    ListContactsHandlerTests.cs                         [Task 5]
    SearchContactsHandlerTests.cs                       [Task 5]
    AddContactHandlerTests.cs                           [Task 5]
    UpdateContactHandlerTests.cs                        [Task 5]
    DeleteContactHandlerTests.cs                        [Task 5]
```

**Project graph after refactor:**
- `Web` → `Application`, `Infrastructure`
- `Application` → `Domain`
- `Infrastructure` → `Domain`
- `Domain` → (nothing)
- `Application.Tests` → `Application`, `Domain` (NOT `Infrastructure` — load-bearing for hex)

---

## Task 1: Create the Domain project

**Files:**
- Create: `src/HtmxMvc.Domain/HtmxMvc.Domain.csproj`
- Create: `src/HtmxMvc.Domain/Contact.cs`
- Create: `src/HtmxMvc.Domain/IContactRepository.cs`
- Modify: `HtmxMvc.slnx` (add domain project)

- [ ] **Step 1: Create the Domain class library**

Run from `C:\repo\poc\HtmxMvc`:
```powershell
dotnet new classlib -n HtmxMvc.Domain -o src/HtmxMvc.Domain --framework net10.0
Remove-Item src/HtmxMvc.Domain/Class1.cs
dotnet sln HtmxMvc.slnx add src/HtmxMvc.Domain/HtmxMvc.Domain.csproj
```

Expected: project created and added to slnx.

- [ ] **Step 2: Write the Contact entity**

Replace contents of `src/HtmxMvc.Domain/Contact.cs`:
```csharp
namespace HtmxMvc.Domain;

public sealed class Contact
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
}
```

No data annotations — domain is anemic and presentation-agnostic.

- [ ] **Step 3: Write the IContactRepository port**

Create `src/HtmxMvc.Domain/IContactRepository.cs`:
```csharp
namespace HtmxMvc.Domain;

public interface IContactRepository
{
    Task<IReadOnlyList<Contact>> GetAllAsync(CancellationToken ct = default);
    Task<Contact?> GetAsync(int id, CancellationToken ct = default);
    Task<Contact> AddAsync(Contact contact, CancellationToken ct = default);
    Task<Contact?> UpdateAsync(int id, Contact contact, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
```

- [ ] **Step 4: Verify build**

Run: `dotnet build src/HtmxMvc.Domain`
Expected: `Build succeeded` with 0 errors.

- [ ] **Step 5: Commit**

```powershell
git add HtmxMvc.slnx src/HtmxMvc.Domain
git commit -m "$(@'
feat: add Domain project with Contact entity and IContactRepository port

Co-Authored-By: Claude Opus 4.7 (1M context) <noreply@anthropic.com>
'@)"
```

---

## Task 2: Create the Infrastructure project

**Files:**
- Create: `src/HtmxMvc.Infrastructure/HtmxMvc.Infrastructure.csproj`
- Create: `src/HtmxMvc.Infrastructure/Contacts/InMemoryContactRepository.cs`
- Create: `src/HtmxMvc.Infrastructure/DependencyInjection.cs`
- Modify: `HtmxMvc.slnx` (add infrastructure project)

- [ ] **Step 1: Create the Infrastructure class library and link Domain**

```powershell
dotnet new classlib -n HtmxMvc.Infrastructure -o src/HtmxMvc.Infrastructure --framework net10.0
Remove-Item src/HtmxMvc.Infrastructure/Class1.cs
dotnet sln HtmxMvc.slnx add src/HtmxMvc.Infrastructure/HtmxMvc.Infrastructure.csproj
dotnet add src/HtmxMvc.Infrastructure reference src/HtmxMvc.Domain
dotnet add src/HtmxMvc.Infrastructure package Microsoft.Extensions.DependencyInjection.Abstractions
```

Expected: project created, Domain referenced, DI Abstractions package added.

- [ ] **Step 2: Write the in-memory repository**

Create `src/HtmxMvc.Infrastructure/Contacts/InMemoryContactRepository.cs`:
```csharp
using HtmxMvc.Domain;

namespace HtmxMvc.Infrastructure.Contacts;

public sealed class InMemoryContactRepository : IContactRepository
{
    private readonly Lock _gate = new();
    private readonly List<Contact> _contacts = [];
    private int _nextId = 1;

    public InMemoryContactRepository()
    {
        Seed(new Contact { Name = "Ada Lovelace",      Email = "ada@analyticalengine.org",  Phone = "555-0101" });
        Seed(new Contact { Name = "Alan Turing",       Email = "alan@bletchley.uk",         Phone = "555-0102" });
        Seed(new Contact { Name = "Grace Hopper",      Email = "grace@navy.mil",            Phone = "555-0103" });
        Seed(new Contact { Name = "Edsger Dijkstra",   Email = "edsger@eindhoven.nl",       Phone = "555-0104" });
        Seed(new Contact { Name = "Margaret Hamilton", Email = "margaret@apollo.nasa.gov",  Phone = "555-0105" });
    }

    private void Seed(Contact c)
    {
        c.Id = _nextId++;
        _contacts.Add(c);
    }

    public Task<IReadOnlyList<Contact>> GetAllAsync(CancellationToken ct = default)
    {
        lock (_gate)
        {
            IReadOnlyList<Contact> snapshot = _contacts.OrderBy(c => c.Id).ToList();
            return Task.FromResult(snapshot);
        }
    }

    public Task<Contact?> GetAsync(int id, CancellationToken ct = default)
    {
        lock (_gate)
        {
            return Task.FromResult(_contacts.FirstOrDefault(c => c.Id == id));
        }
    }

    public Task<Contact> AddAsync(Contact contact, CancellationToken ct = default)
    {
        lock (_gate)
        {
            contact.Id = _nextId++;
            _contacts.Add(contact);
            return Task.FromResult(contact);
        }
    }

    public Task<Contact?> UpdateAsync(int id, Contact contact, CancellationToken ct = default)
    {
        lock (_gate)
        {
            var existing = _contacts.FirstOrDefault(c => c.Id == id);
            if (existing is null) return Task.FromResult<Contact?>(null);
            existing.Name = contact.Name;
            existing.Email = contact.Email;
            existing.Phone = contact.Phone;
            return Task.FromResult<Contact?>(existing);
        }
    }

    public Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        lock (_gate)
        {
            var existing = _contacts.FirstOrDefault(c => c.Id == id);
            if (existing is null) return Task.FromResult(false);
            _contacts.Remove(existing);
            return Task.FromResult(true);
        }
    }
}
```

- [ ] **Step 3: Add the DI extension**

Create `src/HtmxMvc.Infrastructure/DependencyInjection.cs`:
```csharp
using HtmxMvc.Domain;
using HtmxMvc.Infrastructure.Contacts;
using Microsoft.Extensions.DependencyInjection;

namespace HtmxMvc.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IContactRepository, InMemoryContactRepository>();
        return services;
    }
}
```

- [ ] **Step 4: Verify build**

Run: `dotnet build src/HtmxMvc.Infrastructure`
Expected: `Build succeeded` with 0 errors.

- [ ] **Step 5: Commit**

```powershell
git add HtmxMvc.slnx src/HtmxMvc.Infrastructure
git commit -m "$(@'
feat: add Infrastructure project with InMemoryContactRepository

Implements IContactRepository as a thread-safe singleton, seeded with
the same five contacts as the original ContactService.

Co-Authored-By: Claude Opus 4.7 (1M context) <noreply@anthropic.com>
'@)"
```

---

## Task 3: Create the Application project

**Files:**
- Create: `src/HtmxMvc.Application/HtmxMvc.Application.csproj`
- Create: `src/HtmxMvc.Application/Contacts/ContactInput.cs`
- Create: `src/HtmxMvc.Application/Contacts/ListContactsHandler.cs`
- Create: `src/HtmxMvc.Application/Contacts/SearchContactsHandler.cs`
- Create: `src/HtmxMvc.Application/Contacts/GetContactHandler.cs`
- Create: `src/HtmxMvc.Application/Contacts/AddContactHandler.cs`
- Create: `src/HtmxMvc.Application/Contacts/UpdateContactHandler.cs`
- Create: `src/HtmxMvc.Application/Contacts/DeleteContactHandler.cs`
- Create: `src/HtmxMvc.Application/DependencyInjection.cs`
- Modify: `HtmxMvc.slnx`

- [ ] **Step 1: Create the Application class library and link Domain**

```powershell
dotnet new classlib -n HtmxMvc.Application -o src/HtmxMvc.Application --framework net10.0
Remove-Item src/HtmxMvc.Application/Class1.cs
dotnet sln HtmxMvc.slnx add src/HtmxMvc.Application/HtmxMvc.Application.csproj
dotnet add src/HtmxMvc.Application reference src/HtmxMvc.Domain
dotnet add src/HtmxMvc.Application package Microsoft.Extensions.DependencyInjection.Abstractions
```

- [ ] **Step 2: Write ContactInput**

Create `src/HtmxMvc.Application/Contacts/ContactInput.cs`:
```csharp
using System.ComponentModel.DataAnnotations;

namespace HtmxMvc.Application.Contacts;

public sealed record ContactInput
{
    [Required, StringLength(100)]
    public string Name { get; init; } = "";

    [StringLength(200)]
    public string Email { get; init; } = "";

    [StringLength(50)]
    public string Phone { get; init; } = "";
}
```

- [ ] **Step 3: Write the six handler classes**

Create `src/HtmxMvc.Application/Contacts/ListContactsHandler.cs`:
```csharp
using HtmxMvc.Domain;

namespace HtmxMvc.Application.Contacts;

public sealed class ListContactsHandler(IContactRepository repo)
{
    public Task<IReadOnlyList<Contact>> ExecuteAsync(CancellationToken ct = default)
        => repo.GetAllAsync(ct);
}
```

Create `src/HtmxMvc.Application/Contacts/SearchContactsHandler.cs`:
```csharp
using HtmxMvc.Domain;

namespace HtmxMvc.Application.Contacts;

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

Create `src/HtmxMvc.Application/Contacts/GetContactHandler.cs`:
```csharp
using HtmxMvc.Domain;

namespace HtmxMvc.Application.Contacts;

public sealed class GetContactHandler(IContactRepository repo)
{
    public Task<Contact?> ExecuteAsync(int id, CancellationToken ct = default)
        => repo.GetAsync(id, ct);
}
```

Create `src/HtmxMvc.Application/Contacts/AddContactHandler.cs`:
```csharp
using HtmxMvc.Domain;

namespace HtmxMvc.Application.Contacts;

public sealed class AddContactHandler(IContactRepository repo)
{
    public Task<Contact> ExecuteAsync(ContactInput input, CancellationToken ct = default)
    {
        var contact = new Contact
        {
            Name = input.Name,
            Email = input.Email,
            Phone = input.Phone
        };
        return repo.AddAsync(contact, ct);
    }
}
```

Create `src/HtmxMvc.Application/Contacts/UpdateContactHandler.cs`:
```csharp
using HtmxMvc.Domain;

namespace HtmxMvc.Application.Contacts;

public sealed class UpdateContactHandler(IContactRepository repo)
{
    public Task<Contact?> ExecuteAsync(int id, ContactInput input, CancellationToken ct = default)
    {
        var contact = new Contact
        {
            Name = input.Name,
            Email = input.Email,
            Phone = input.Phone
        };
        return repo.UpdateAsync(id, contact, ct);
    }
}
```

Create `src/HtmxMvc.Application/Contacts/DeleteContactHandler.cs`:
```csharp
using HtmxMvc.Domain;

namespace HtmxMvc.Application.Contacts;

public sealed class DeleteContactHandler(IContactRepository repo)
{
    public Task<bool> ExecuteAsync(int id, CancellationToken ct = default)
        => repo.DeleteAsync(id, ct);
}
```

- [ ] **Step 4: Write the Application DI extension**

Create `src/HtmxMvc.Application/DependencyInjection.cs`:
```csharp
using HtmxMvc.Application.Contacts;
using Microsoft.Extensions.DependencyInjection;

namespace HtmxMvc.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ListContactsHandler>();
        services.AddScoped<SearchContactsHandler>();
        services.AddScoped<GetContactHandler>();
        services.AddScoped<AddContactHandler>();
        services.AddScoped<UpdateContactHandler>();
        services.AddScoped<DeleteContactHandler>();
        return services;
    }
}
```

- [ ] **Step 5: Verify build**

Run: `dotnet build src/HtmxMvc.Application`
Expected: `Build succeeded` with 0 errors.

- [ ] **Step 6: Commit**

```powershell
git add HtmxMvc.slnx src/HtmxMvc.Application
git commit -m "$(@'
feat: add Application project with one handler per use case

Six handlers (List/Search/Get/Add/Update/Delete) orchestrate calls to
IContactRepository. SearchContactsHandler owns the case-insensitive
contains predicate so the handler has real responsibility instead of
being a pass-through. ContactInput is the write-side DTO; the Domain
Contact entity stays anemic.

Co-Authored-By: Claude Opus 4.7 (1M context) <noreply@anthropic.com>
'@)"
```

---

## Task 4: Rewire the Web project as the composition root

This task renames the existing project and rewires controllers/views. Verification happens at the end.

**Files:**
- Rename: `src/HtmxMvc/` → `src/HtmxMvc.Web/`
- Rename: `src/HtmxMvc.Web/HtmxMvc.csproj` → `src/HtmxMvc.Web/HtmxMvc.Web.csproj`
- Modify: `HtmxMvc.slnx`
- Modify: `src/HtmxMvc.Web/HtmxMvc.Web.csproj` (add ProjectReferences, set RootNamespace)
- Modify: `src/HtmxMvc.Web/Program.cs`
- Modify: `src/HtmxMvc.Web/Controllers/ContactsController.cs`
- Modify: `src/HtmxMvc.Web/Views/_ViewImports.cshtml`
- Modify: `src/HtmxMvc.Web/Views/Shared/_ContactRow.cshtml`
- Modify: `src/HtmxMvc.Web/Views/Shared/_ContactEditRow.cshtml`
- Modify: `src/HtmxMvc.Web/Views/Shared/_ContactList.cshtml`
- Modify: `src/HtmxMvc.Web/Views/Contacts/Index.cshtml`
- Delete: `src/HtmxMvc.Web/Models/Contact.cs` (and empty `Models/` directory)
- Delete: `src/HtmxMvc.Web/Services/ContactService.cs` (and empty `Services/` directory)

- [ ] **Step 1: Remove old project from solution and clean its build outputs**

```powershell
dotnet sln HtmxMvc.slnx remove src/HtmxMvc/HtmxMvc.csproj
Remove-Item -Recurse -Force src/HtmxMvc/bin, src/HtmxMvc/obj -ErrorAction SilentlyContinue
```

- [ ] **Step 2: Rename the directory and csproj**

```powershell
Rename-Item src/HtmxMvc src/HtmxMvc.Web
Rename-Item src/HtmxMvc.Web/HtmxMvc.csproj HtmxMvc.Web.csproj
dotnet sln HtmxMvc.slnx add src/HtmxMvc.Web/HtmxMvc.Web.csproj
```

- [ ] **Step 3: Update HtmxMvc.Web.csproj**

Replace contents of `src/HtmxMvc.Web/HtmxMvc.Web.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>HtmxMvc</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\HtmxMvc.Application\HtmxMvc.Application.csproj" />
    <ProjectReference Include="..\HtmxMvc.Infrastructure\HtmxMvc.Infrastructure.csproj" />
  </ItemGroup>

</Project>
```

`<RootNamespace>HtmxMvc</RootNamespace>` keeps existing `namespace HtmxMvc.Controllers` etc. files working without renaming.

- [ ] **Step 4: Rewrite Program.cs**

Replace contents of `src/HtmxMvc.Web/Program.cs`:
```csharp
using HtmxMvc.Application;
using HtmxMvc.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllers();

app.Run();
```

- [ ] **Step 5: Rewrite ContactsController.cs**

Replace contents of `src/HtmxMvc.Web/Controllers/ContactsController.cs`:
```csharp
using HtmxMvc.Application.Contacts;
using Microsoft.AspNetCore.Mvc;

namespace HtmxMvc.Controllers;

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

    [HttpGet("/contacts/list")]
    public async Task<IActionResult> List(string? q, CancellationToken ct)
        => PartialView("_ContactList", await search.ExecuteAsync(q, ct));

    [HttpPost("/contacts")]
    public async Task<IActionResult> Create(ContactInput input, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest();
        var created = await add.ExecuteAsync(input, ct);
        return PartialView("_ContactRow", created);
    }

    [HttpGet("/contacts/{id:int}/edit")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var c = await get.ExecuteAsync(id, ct);
        return c is null ? NotFound() : PartialView("_ContactEditRow", c);
    }

    [HttpPut("/contacts/{id:int}")]
    public async Task<IActionResult> Update(int id, ContactInput input, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest();
        var updated = await update.ExecuteAsync(id, input, ct);
        return updated is null ? NotFound() : PartialView("_ContactRow", updated);
    }

    [HttpGet("/contacts/{id:int}")]
    public async Task<IActionResult> Row(int id, CancellationToken ct)
    {
        var c = await get.ExecuteAsync(id, ct);
        return c is null ? NotFound() : PartialView("_ContactRow", c);
    }

    [HttpDelete("/contacts/{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => await delete.ExecuteAsync(id, ct) ? Ok() : NotFound();
}
```

- [ ] **Step 6: Update _ViewImports.cshtml**

Replace contents of `src/HtmxMvc.Web/Views/_ViewImports.cshtml`:
```cshtml
@using HtmxMvc
@using HtmxMvc.Domain
@using HtmxMvc.Application.Contacts
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

- [ ] **Step 7: Update the four views to bind to HtmxMvc.Domain.Contact**

In `src/HtmxMvc.Web/Views/Shared/_ContactRow.cshtml`, change the `@model` line to:
```cshtml
@model HtmxMvc.Domain.Contact
```

In `src/HtmxMvc.Web/Views/Shared/_ContactEditRow.cshtml`, change the `@model` line to:
```cshtml
@model HtmxMvc.Domain.Contact
```

In `src/HtmxMvc.Web/Views/Shared/_ContactList.cshtml`, change the `@model` line to:
```cshtml
@model IReadOnlyList<HtmxMvc.Domain.Contact>
```

In `src/HtmxMvc.Web/Views/Contacts/Index.cshtml`, change the `@model` line to:
```cshtml
@model IReadOnlyList<HtmxMvc.Domain.Contact>
```

Only the `@model` line changes; HTMX attributes and Tailwind classes are unchanged.

- [ ] **Step 8: Delete the obsolete Models and Services folders**

```powershell
Remove-Item -Recurse -Force src/HtmxMvc.Web/Models
Remove-Item -Recurse -Force src/HtmxMvc.Web/Services
```

- [ ] **Step 9: Build the whole solution**

Run: `dotnet build`
Expected: `Build succeeded` with 0 errors. If you see CS0246 about `Models` or `Services`, you missed a `@using` in `_ViewImports.cshtml` — see Step 6.

- [ ] **Step 10: Smoke test the running app**

Start the app in the background:
```powershell
dotnet run --project src/HtmxMvc.Web --urls http://localhost:5099
```

Wait for `Now listening on: http://localhost:5099`, then:
```powershell
curl http://localhost:5099/ -o response.html
Select-String -Path response.html -Pattern "Ada Lovelace|Alan Turing|Grace Hopper|Edsger Dijkstra|Margaret Hamilton"
curl "http://localhost:5099/contacts/list?q=ada"
curl "http://localhost:5099/contacts/1"
```

Expected:
- `curl /` → 200, all five names present in response.html
- `/contacts/list?q=ada` → 200, contains `<tr id="contact-1">` and `Ada Lovelace`
- `/contacts/1` → 200, single row partial

Stop the app (Ctrl+C the background task).

- [ ] **Step 11: Commit**

```powershell
git add HtmxMvc.slnx src/HtmxMvc.Web
git add -u  # picks up the deletes
git commit -m "$(@'
refactor: rewire Web as composition root for hex layers

- Renames the project HtmxMvc -> HtmxMvc.Web; keeps RootNamespace=HtmxMvc
  so existing C# namespaces and views compile unchanged.
- ContactsController now takes the six handlers via primary constructor;
  every action is async and accepts a CancellationToken.
- Views bind to HtmxMvc.Domain.Contact (no behavior change).
- ContactInput replaces Contact for POST/PUT model binding so [Required]
  no longer leaks into the Domain entity.
- Deletes the obsolete Models/Contact.cs and Services/ContactService.cs.

Co-Authored-By: Claude Opus 4.7 (1M context) <noreply@anthropic.com>
'@)"
```

---

## Task 5: Create the Application.Tests project

**Files:**
- Create: `tests/HtmxMvc.Application.Tests/HtmxMvc.Application.Tests.csproj`
- Create: `tests/HtmxMvc.Application.Tests/FakeContactRepository.cs`
- Create: `tests/HtmxMvc.Application.Tests/ListContactsHandlerTests.cs`
- Create: `tests/HtmxMvc.Application.Tests/SearchContactsHandlerTests.cs`
- Create: `tests/HtmxMvc.Application.Tests/AddContactHandlerTests.cs`
- Create: `tests/HtmxMvc.Application.Tests/UpdateContactHandlerTests.cs`
- Create: `tests/HtmxMvc.Application.Tests/DeleteContactHandlerTests.cs`
- Modify: `HtmxMvc.slnx`

- [ ] **Step 1: Create the test project**

```powershell
dotnet new classlib -n HtmxMvc.Application.Tests -o tests/HtmxMvc.Application.Tests --framework net10.0
Remove-Item tests/HtmxMvc.Application.Tests/Class1.cs
dotnet sln HtmxMvc.slnx add tests/HtmxMvc.Application.Tests/HtmxMvc.Application.Tests.csproj
dotnet add tests/HtmxMvc.Application.Tests reference src/HtmxMvc.Application
dotnet add tests/HtmxMvc.Application.Tests reference src/HtmxMvc.Domain
```

NOTE: Do NOT add a reference to `src/HtmxMvc.Infrastructure` — that would defeat the purpose of testing the Application layer in isolation.

- [ ] **Step 2: Add xUnit.v3 packages**

```powershell
dotnet add tests/HtmxMvc.Application.Tests package xunit.v3
dotnet add tests/HtmxMvc.Application.Tests package xunit.v3.runner.visualstudio
dotnet add tests/HtmxMvc.Application.Tests package Microsoft.NET.Test.Sdk
```

Then open `tests/HtmxMvc.Application.Tests/HtmxMvc.Application.Tests.csproj` and add `<IsPackable>false</IsPackable>` and `<IsTestProject>true</IsTestProject>` inside `<PropertyGroup>`. Final shape (verify it matches; PackageReference versions will be whatever `dotnet add` chose):
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="..." />
    <PackageReference Include="xunit.v3" Version="..." />
    <PackageReference Include="xunit.v3.runner.visualstudio" Version="..." />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\HtmxMvc.Application\HtmxMvc.Application.csproj" />
    <ProjectReference Include="..\..\src\HtmxMvc.Domain\HtmxMvc.Domain.csproj" />
  </ItemGroup>

</Project>
```

- [ ] **Step 3: Write the FakeContactRepository**

Create `tests/HtmxMvc.Application.Tests/FakeContactRepository.cs`:
```csharp
using HtmxMvc.Domain;

namespace HtmxMvc.Application.Tests;

internal sealed class FakeContactRepository : IContactRepository
{
    private readonly List<Contact> _contacts = [];
    private int _nextId = 1;

    public FakeContactRepository(params Contact[] seed)
    {
        foreach (var c in seed)
        {
            c.Id = _nextId++;
            _contacts.Add(c);
        }
    }

    public Task<IReadOnlyList<Contact>> GetAllAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<Contact>>(_contacts.OrderBy(c => c.Id).ToList());

    public Task<Contact?> GetAsync(int id, CancellationToken ct = default)
        => Task.FromResult(_contacts.FirstOrDefault(c => c.Id == id));

    public Task<Contact> AddAsync(Contact contact, CancellationToken ct = default)
    {
        contact.Id = _nextId++;
        _contacts.Add(contact);
        return Task.FromResult(contact);
    }

    public Task<Contact?> UpdateAsync(int id, Contact contact, CancellationToken ct = default)
    {
        var existing = _contacts.FirstOrDefault(c => c.Id == id);
        if (existing is null) return Task.FromResult<Contact?>(null);
        existing.Name = contact.Name;
        existing.Email = contact.Email;
        existing.Phone = contact.Phone;
        return Task.FromResult<Contact?>(existing);
    }

    public Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var existing = _contacts.FirstOrDefault(c => c.Id == id);
        if (existing is null) return Task.FromResult(false);
        _contacts.Remove(existing);
        return Task.FromResult(true);
    }
}
```

- [ ] **Step 4: Write ListContactsHandlerTests**

Create `tests/HtmxMvc.Application.Tests/ListContactsHandlerTests.cs`:
```csharp
using HtmxMvc.Application.Contacts;
using HtmxMvc.Domain;
using Xunit;

namespace HtmxMvc.Application.Tests;

public class ListContactsHandlerTests
{
    [Fact]
    public async Task Returns_all_contacts_in_id_order()
    {
        var repo = new FakeContactRepository(
            new Contact { Name = "Bea" },
            new Contact { Name = "Ana" });
        var handler = new ListContactsHandler(repo);

        var result = await handler.ExecuteAsync();

        Assert.Equal(new[] { "Bea", "Ana" }, result.Select(c => c.Name));
    }

    [Fact]
    public async Task Returns_empty_when_repo_is_empty()
    {
        var handler = new ListContactsHandler(new FakeContactRepository());
        var result = await handler.ExecuteAsync();
        Assert.Empty(result);
    }
}
```

- [ ] **Step 5: Write SearchContactsHandlerTests (the most important test file)**

Create `tests/HtmxMvc.Application.Tests/SearchContactsHandlerTests.cs`:
```csharp
using HtmxMvc.Application.Contacts;
using HtmxMvc.Domain;
using Xunit;

namespace HtmxMvc.Application.Tests;

public class SearchContactsHandlerTests
{
    private static FakeContactRepository SeededRepo() => new(
        new Contact { Name = "Ada Lovelace",     Email = "ada@analyticalengine.org",  Phone = "555-0101" },
        new Contact { Name = "Alan Turing",      Email = "alan@bletchley.uk",         Phone = "555-0102" },
        new Contact { Name = "Grace Hopper",     Email = "grace@navy.mil",            Phone = "555-0103" });

    [Fact]
    public async Task Returns_all_when_query_is_null()
    {
        var handler = new SearchContactsHandler(SeededRepo());
        var result = await handler.ExecuteAsync(null);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task Returns_all_when_query_is_whitespace()
    {
        var handler = new SearchContactsHandler(SeededRepo());
        var result = await handler.ExecuteAsync("   ");
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task Matches_name_case_insensitively()
    {
        var handler = new SearchContactsHandler(SeededRepo());
        var result = await handler.ExecuteAsync("ADA");
        Assert.Single(result);
        Assert.Equal("Ada Lovelace", result[0].Name);
    }

    [Fact]
    public async Task Matches_email_substring()
    {
        var handler = new SearchContactsHandler(SeededRepo());
        var result = await handler.ExecuteAsync("bletchley");
        Assert.Single(result);
        Assert.Equal("Alan Turing", result[0].Name);
    }

    [Fact]
    public async Task Matches_phone_substring()
    {
        var handler = new SearchContactsHandler(SeededRepo());
        var result = await handler.ExecuteAsync("0103");
        Assert.Single(result);
        Assert.Equal("Grace Hopper", result[0].Name);
    }

    [Fact]
    public async Task Returns_empty_when_no_match()
    {
        var handler = new SearchContactsHandler(SeededRepo());
        var result = await handler.ExecuteAsync("zzz");
        Assert.Empty(result);
    }
}
```

- [ ] **Step 6: Write AddContactHandlerTests**

Create `tests/HtmxMvc.Application.Tests/AddContactHandlerTests.cs`:
```csharp
using HtmxMvc.Application.Contacts;
using Xunit;

namespace HtmxMvc.Application.Tests;

public class AddContactHandlerTests
{
    [Fact]
    public async Task Adds_contact_and_assigns_id()
    {
        var repo = new FakeContactRepository();
        var handler = new AddContactHandler(repo);

        var created = await handler.ExecuteAsync(new ContactInput
        {
            Name = "Linus Torvalds",
            Email = "linus@kernel.org",
            Phone = "555-0106"
        });

        Assert.True(created.Id > 0);
        Assert.Equal("Linus Torvalds", created.Name);

        var all = await repo.GetAllAsync();
        Assert.Single(all);
    }
}
```

- [ ] **Step 7: Write UpdateContactHandlerTests**

Create `tests/HtmxMvc.Application.Tests/UpdateContactHandlerTests.cs`:
```csharp
using HtmxMvc.Application.Contacts;
using HtmxMvc.Domain;
using Xunit;

namespace HtmxMvc.Application.Tests;

public class UpdateContactHandlerTests
{
    [Fact]
    public async Task Updates_existing_contact()
    {
        var repo = new FakeContactRepository(
            new Contact { Name = "Old", Email = "old@x.com", Phone = "1" });
        var handler = new UpdateContactHandler(repo);

        var updated = await handler.ExecuteAsync(1, new ContactInput
        {
            Name = "New",
            Email = "new@x.com",
            Phone = "2"
        });

        Assert.NotNull(updated);
        Assert.Equal("New", updated!.Name);
        Assert.Equal("new@x.com", updated.Email);
        Assert.Equal("2", updated.Phone);
    }

    [Fact]
    public async Task Returns_null_when_id_not_found()
    {
        var repo = new FakeContactRepository();
        var handler = new UpdateContactHandler(repo);

        var result = await handler.ExecuteAsync(99, new ContactInput { Name = "x" });

        Assert.Null(result);
    }
}
```

- [ ] **Step 8: Write DeleteContactHandlerTests**

Create `tests/HtmxMvc.Application.Tests/DeleteContactHandlerTests.cs`:
```csharp
using HtmxMvc.Application.Contacts;
using HtmxMvc.Domain;
using Xunit;

namespace HtmxMvc.Application.Tests;

public class DeleteContactHandlerTests
{
    [Fact]
    public async Task Deletes_existing_contact()
    {
        var repo = new FakeContactRepository(new Contact { Name = "Doomed" });
        var handler = new DeleteContactHandler(repo);

        var deleted = await handler.ExecuteAsync(1);

        Assert.True(deleted);
        Assert.Empty(await repo.GetAllAsync());
    }

    [Fact]
    public async Task Returns_false_when_id_not_found()
    {
        var repo = new FakeContactRepository();
        var handler = new DeleteContactHandler(repo);

        var deleted = await handler.ExecuteAsync(99);

        Assert.False(deleted);
    }
}
```

- [ ] **Step 9: Run the tests**

Run: `dotnet test`
Expected:
- All tests pass (~14 tests).
- Output shows `Passed! - Failed: 0, Passed: 14, Skipped: 0`.

If anything fails, run `dotnet test --logger "console;verbosity=detailed"` to see which assertion broke. Also double-check the csproj: it should reference `HtmxMvc.Application` and `HtmxMvc.Domain` only. If `HtmxMvc.Infrastructure` slipped in (e.g. from autocomplete), tests will still pass but the hex isolation guarantee is silently broken — remove it.

- [ ] **Step 10: Commit**

```powershell
git add HtmxMvc.slnx tests/HtmxMvc.Application.Tests
git commit -m "$(@'
test: add Application.Tests with xUnit.v3 and a fake repository

Tests cover the five non-trivial handlers (search, add, update, delete,
list). FakeContactRepository is hand-rolled, ~30 lines, so the test
project does not need Moq or any Infrastructure reference. The absence
of that reference is the load-bearing proof that the Application layer
can be tested in isolation.

Co-Authored-By: Claude Opus 4.7 (1M context) <noreply@anthropic.com>
'@)"
```

---

## Task 6: Full-stack verification

**Files:** none (verification only)

- [ ] **Step 1: Build clean and run tests**

```powershell
dotnet build
dotnet test
```

Expected: build succeeded, all tests pass.

- [ ] **Step 2: Run the app and smoke-test all six HTMX patterns**

Start the app in the background:
```powershell
dotnet run --project src/HtmxMvc.Web --urls http://localhost:5099
```

Wait for `Now listening on: http://localhost:5099`, then run the same end-to-end CRUD check used after the original build (extracts antiforgery token from `<body hx-headers>`, then POST/PUT/DELETE):

```powershell
$jar = "$env:TEMP\hex-cookies.txt"
if (Test-Path $jar) { Remove-Item $jar }
$html = & curl.exe -sc $jar http://localhost:5099/
$token = ($html | Select-String -Pattern 'RequestVerificationToken&quot;: &quot;([A-Za-z0-9_\-]+)' -AllMatches).Matches[0].Groups[1].Value

"POST contacts:"
& curl.exe -sb $jar -X POST "http://localhost:5099/contacts" -H "RequestVerificationToken: $token" --data-urlencode "Name=Linus Torvalds" --data-urlencode "Email=linus@kernel.org" --data-urlencode "Phone=555-0106" -w "status=%{http_code}`n" -o new.html
$newId = ($null = (Select-String -Path new.html -Pattern 'id="contact-(\d+)"').Matches[0].Groups[1].Value); $newId

"PUT contacts/$newId :"
& curl.exe -sb $jar -X PUT "http://localhost:5099/contacts/$newId" -H "RequestVerificationToken: $token" --data-urlencode "Name=Linus B Torvalds" --data-urlencode "Email=linus@kernel.org" --data-urlencode "Phone=555-0106" -w "status=%{http_code}`n" -o upd.html

"DELETE contacts/$newId :"
& curl.exe -sb $jar -X DELETE "http://localhost:5099/contacts/$newId" -H "RequestVerificationToken: $token" -w "status=%{http_code}`n" -o $null

"Verify Linus gone:"
$listing = & curl.exe -s "http://localhost:5099/contacts/list"
if ($listing -match "Linus") { "FAIL" } else { "OK" }
```

Expected:
- POST → status=200, returns a `<tr id="contact-6">` partial
- PUT → status=200, response contains "Linus B Torvalds"
- DELETE → status=200, empty body
- Final listing → `OK` (Linus not present)

Stop the app.

- [ ] **Step 3: Update README to reflect the new structure**

Replace the `## Layout` section of `README.md` with:
```markdown
## Layout

| Path | Responsibility |
|---|---|
| `src/HtmxMvc.Domain/` | `Contact` entity, `IContactRepository` port |
| `src/HtmxMvc.Application/` | One handler per use case, `ContactInput` DTO, `AddApplication()` DI extension |
| `src/HtmxMvc.Infrastructure/` | `InMemoryContactRepository` (thread-safe singleton), `AddInfrastructure()` |
| `src/HtmxMvc.Web/` | Composition root: `Program.cs` calls `AddApplication()` + `AddInfrastructure()`; `ContactsController` injects the six handlers |
| `tests/HtmxMvc.Application.Tests/` | xUnit.v3 tests against `FakeContactRepository` |

Project graph: `Web` → `Application` + `Infrastructure`; `Application` and `Infrastructure` both → `Domain`. `Application.Tests` references `Application` and `Domain` only.
```

Also change the **Run** command from:
```markdown
dotnet run --project src/HtmxMvc
```
to:
```markdown
dotnet run --project src/HtmxMvc.Web
```

- [ ] **Step 4: Commit**

```powershell
git add README.md
git commit -m "$(@'
docs: update README layout for hex project structure

Co-Authored-By: Claude Opus 4.7 (1M context) <noreply@anthropic.com>
'@)"
```

---

## Done

After Task 6, the demo runs identically to before but is now organized as four projects plus tests. To explore:

```powershell
dotnet test
dotnet run --project src/HtmxMvc.Web
```

The original browser-MCP test sequence (in `docs/superpowers/screenshots/`) should still pass with no visible differences.
