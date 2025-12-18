using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SupportDeskBE.Models;

namespace SupportDeskBE.Data
{
    public class SupportDeskDbContext : IdentityDbContext
    {
        public SupportDeskDbContext(DbContextOptions<SupportDeskDbContext> options)
            : base(options) { }

        public DbSet<Ticket> Tickets => Set<Ticket>();
        public DbSet<Message> Messages => Set<Message>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Ticket>()
                .HasMany(t => t.Messages)
                .WithOne(m => m.Ticket)
                .HasForeignKey(m => m.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            // Helpful indexes for later (safe to add now)
            builder.Entity<Ticket>()
                .HasIndex(t => t.Status);

            builder.Entity<Ticket>()
                .HasIndex(t => t.Priority);

            builder.Entity<Ticket>()
                .HasIndex(t => t.UpdatedAt);
        }
    }
}

