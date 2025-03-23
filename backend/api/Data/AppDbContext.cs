using api.Helpers;
using api.Helpers.Instances;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options, IHasher passwordHasher)
        : DbContext(options)
    {
        private readonly IHasher _passwordHasher = passwordHasher;
        public DbSet<User> Users { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // modelBuilder.Entity<Reservation>().HasIndex(r => new { r.Id }).IsUnique();
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

            // modelBuilder.Entity<Room>().HasIndex(r => new { r.Id }).IsUnique();

            modelBuilder.Entity<User>().HasIndex(r => new { r.Username }).IsUnique();
            modelBuilder.Entity<User>().HasIndex(r => new { r.Email }).IsUnique();

            // modelBuilder.Entity<Role>().HasIndex(r => new { r.Name }).IsUnique();
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

        public async Task SeedBasicUsers(AppDbContext context)
        {
            if (!context.Users.Any(u => u.Username == "admin"))
            {
                using var transaction = await context.Database.BeginTransactionAsync();
                try
                {
                    List<string> roleNames = ["Admin", "User"];
                    List<Role> filteredData = [];
                    List<Role> existRole = await context
                        .Roles.Where(r => roleNames.Contains(r.Name))
                        .ToListAsync();
                    if (!(existRole.Count == 2))
                    {
                        List<string> createdNameRoles = [.. existRole.Select(r => r.Name)];
                        filteredData =
                        [
                            .. roleNames
                                .Where(r => !createdNameRoles.Contains(r))
                                .Select(r => new Role { Name = r }),
                        ];
                        await context.AddRangeAsync(filteredData);
                        await context.SaveChangesAsync();
                    }
                    List<Role> roles = [.. existRole, .. filteredData];
                    var adminUser = new User
                    {
                        Username = "admin",
                        Email = "example@example.com",
                        Password = _passwordHasher.HashPassword("admin"),
                        Roles = roles,
                    };
                    await context.Users.AddAsync(adminUser);
                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception error)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("Error during seed handling", error);
                }
                finally
                {
                    await transaction.DisposeAsync();
                }
            }
        }
    }
}
