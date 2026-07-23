using HtmxMvc.Domain;

namespace HtmxMvc.Infrastructure;

public static class DatabaseSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.Contacts.Any()) return;

        db.Contacts.AddRange(
            new Contact { Name = "Ada Lovelace",      Email = "ada@analyticalengine.org",  Phone = "555-0101" },
            new Contact { Name = "Alan Turing",       Email = "alan@bletchley.uk",         Phone = "555-0102" },
            new Contact { Name = "Grace Hopper",      Email = "grace@navy.mil",            Phone = "555-0103" },
            new Contact { Name = "Edsger Dijkstra",   Email = "edsger@eindhoven.nl",       Phone = "555-0104" },
            new Contact { Name = "Margaret Hamilton", Email = "margaret@apollo.nasa.gov",  Phone = "555-0105" });

        db.SaveChanges();
    }
}
