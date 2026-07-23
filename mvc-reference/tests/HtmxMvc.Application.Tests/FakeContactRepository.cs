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
