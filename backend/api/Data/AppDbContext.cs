using api.Helpers;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options, PasswordHasher passwordHasher)
        : DbContext(options)
    {
        private readonly PasswordHasher _passwordHasher = passwordHasher;
        public DbSet<User> Users { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Role> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Reservation>().HasIndex(r => new { r.Id }).IsUnique();
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

            modelBuilder.Entity<Room>().HasIndex(r => new { r.Id }).IsUnique();

            modelBuilder.Entity<User>().HasIndex(r => new { r.Id }).IsUnique();
            modelBuilder
                .Entity<User>()
                .HasMany(r => r.Roles)
                .WithMany(u => u.Users)
                .UsingEntity(j => j.ToTable("UserRoles"));

            modelBuilder.Entity<Payment>().HasIndex(r => new { r.Id }).IsUnique();
            modelBuilder
                .Entity<Payment>()
                .HasOne(r => r.Reservation)
                .WithOne(u => u.Payment)
                .HasForeignKey<Payment>(p => p.ReservationId);

            modelBuilder.Entity<Role>().HasData(new Role { Id = 1, Name = "Admin" });
        }

        public void SeedBasicUsers(AppDbContext context)
        {
            if (!context.Users.Any(u => u.Username == "admin"))
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    var adminRole =
                        context.Roles.Find(1) ?? throw new Exception("Admin role [1] not found");

                    var adminUser = new User
                    {
                        Username = "admin",
                        Email = "example@example.com",
                        Password = _passwordHasher.HashPassword("admin"),
                        Roles = [adminRole],
                    };
                    context.Users.Add(adminUser);
                    context.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception error)
                {
                    transaction.Rollback();
                    throw new Exception("Error during seed handling", error);
                }
            }
        }
    }
}
