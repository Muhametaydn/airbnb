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

        public DbSet<TopReservedHouseViewModel> TopReservedHouseViewModel { get; set; }

        public DbSet<HostStatsViewModel> HostStatsViewModel { get; set; }

        public DbSet<TopSpenderViewModel> TopSpenderViewModel { get; set; }
        //triggelr
        public DbSet<UserUpdateLog> UserUpdateLogs { get; set; }
        public DbSet<HousePriceChangeLog> HousePriceChangeLogs { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Tenant)
                .WithMany(u => u.ReservationsAsTenant) // artık bağlantı var
                .HasForeignKey(r => r.TenantId)
                .OnDelete(DeleteBehavior.Restrict); // ❗ Bu satır artık çalışır


            // Spler icin
            modelBuilder.Entity<TopReservedHouseViewModel>().HasNoKey();
            modelBuilder.Entity<HostStatsViewModel>().HasNoKey();

            //funcklar icin
            modelBuilder.Entity<TopSpenderViewModel>().HasNoKey();

            //triggerler icin
            modelBuilder.Entity<User>()
                   .ToTable("Users", tb => tb.HasTrigger("trg_LogUserUpdate"));

            modelBuilder.Entity<House>()
                    .ToTable("Houses", tb => tb.HasTrigger("trg_HousePriceUpdateLog"));








        }

    }
}
