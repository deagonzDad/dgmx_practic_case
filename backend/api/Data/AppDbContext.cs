using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        protected AppDbContext()
            : base() { }

        public DbSet<User> Users { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Reservation>(builder =>
            {
                builder
                    .HasOne(r => r.User)
                    .WithMany(u => u.Reservations)
                    .HasForeignKey(r => r.UserId);

                builder
                    .HasOne(r => r.Room)
                    .WithMany(r => r.Reservations)
                    .HasForeignKey(r => r.RoomId);
                builder
                    .HasOne(r => r.Payment)
                    .WithOne(r => r.Reservation)
                    .HasForeignKey<Payment>(r => r.ReservationId);
            });
            modelBuilder
                .Entity<User>()
                .HasMany(r => r.Roles)
                .WithMany(u => u.Users)
                .UsingEntity<UserRole>();
        }
    }
}
