using FCG.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace FCG.Infrastructure
{
    public class AppDbContext :  IdentityDbContext
    {
        public DbSet<Client> Clients { get; set; } = null!;

        public DbSet<Game> Games { get; set; } = null!;

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure the Game entity
            builder.Entity<Game>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100); 

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(255); 

                entity.Property(e => e.DateRelease)
                    .IsRequired();

                entity.Property(e => e.DateUpdate)
                    .IsRequired();
            });

            // Configure the Client entity
            builder.Entity<Client>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);                                 
            });

            builder.Entity<Client>()
                .HasOne(c => c.User)
                .WithMany() 
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }


    }
}
