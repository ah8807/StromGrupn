using web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace web.Data
{
    public class StromGrupnContext : IdentityDbContext<ApplicationUser>
    {
        public StromGrupnContext(DbContextOptions<StromGrupnContext> options) : base(options)
        {
        }

        public DbSet<Kalkulator> Kalkulators { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Kalkulator>().ToTable("Izvedba");
        }
    }
}