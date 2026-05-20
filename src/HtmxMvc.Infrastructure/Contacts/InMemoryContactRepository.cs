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
