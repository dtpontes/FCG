using FCG.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FCG.Infrastructure
{
    public class AppDbContext :  IdentityDbContext
    {
        public DbSet<Client> Clients { get; set; } = null!;

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Client>()
                .HasOne(c => c.User)
                .WithMany() // Or .WithMany(u => u.Clients) if you add a Clients collection to IdentityUser
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }


    }
}
