using HtmxMvc.Domain;
using Microsoft.EntityFrameworkCore;

namespace HtmxMvc.Infrastructure.Contacts;

public sealed class EfCoreContactRepository(AppDbContext db) : IContactRepository
{
    public async Task<IReadOnlyList<Contact>> GetAllAsync(CancellationToken ct = default)
        => await db.Contacts.AsNoTracking().OrderBy(c => c.Id).ToListAsync(ct);

    public Task<Contact?> GetAsync(int id, CancellationToken ct = default)
        => db.Contacts.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<Contact> AddAsync(Contact contact, CancellationToken ct = default)
    {
        db.Contacts.Add(contact);
        await db.SaveChangesAsync(ct);
        return contact;
    }

    public async Task<Contact?> UpdateAsync(int id, Contact contact, CancellationToken ct = default)
    {
        var existing = await db.Contacts.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (existing is null) return null;
        existing.Name = contact.Name;
        existing.Email = contact.Email;
        existing.Phone = contact.Phone;
        await db.SaveChangesAsync(ct);
        return existing;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var existing = await db.Contacts.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (existing is null) return false;
        db.Contacts.Remove(existing);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
