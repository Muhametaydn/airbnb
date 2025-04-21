using Microsoft.EntityFrameworkCore;
using airbnb.Models;

namespace airbnb.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<House> Houses { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<HouseAvailability> HouseAvailabilities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Tenant)
                .WithMany(u => u.ReservationsAsTenant) // artık bağlantı var
                .HasForeignKey(r => r.TenantId)
                .OnDelete(DeleteBehavior.Restrict); // ❗ Bu satır artık çalışır
        }

    }
}
