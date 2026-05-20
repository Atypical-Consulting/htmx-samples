using HtmxMvc.Domain;
using Microsoft.EntityFrameworkCore;

namespace HtmxMvc.Infrastructure;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Contact> Contacts => Set<Contact>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Contact>(e =>
        {
            e.ToTable("Contacts");
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).IsRequired().HasMaxLength(200);
            e.Property(c => c.Email).HasMaxLength(200);
            e.Property(c => c.Phone).HasMaxLength(50);
        });
    }
}
