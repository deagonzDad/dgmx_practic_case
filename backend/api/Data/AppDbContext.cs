using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId);
            modelBuilder
                .Entity<Reservation>()
                .HasOne(r => r.Room)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.RoomId);

            modelBuilder
                .Entity<User>()
                .HasMany(r => r.Roles)
                .WithMany(u => u.Users)
                .UsingEntity<UserRole>();

            modelBuilder
                .Entity<Payment>()
                .HasOne(r => r.Reservation)
                .WithOne(u => u.Payment)
                .HasForeignKey<Payment>(p => p.ReservationId);
        }
    }
}
