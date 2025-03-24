using System;
using api.Data;
using api.Helpers;
using api.Helpers.Instances;
using api.Models;
using api.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace api.Infrastructure.Data;

public class DatabaseSeeder(
    AppDbContext context,
    IHasher passwordHasher,
    ILogger<AppDbContext> logger,
    IRoleRepository roleRepository,
    IUserRepository userRepository
)
{
    private readonly IHasher _passwordHasher = passwordHasher;
    private readonly IRoleRepository _roleRepository = roleRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ILogger<AppDbContext> _logger = logger;
    private readonly AppDbContext _context = context;

    public async Task SeedBasicUsers()
    {
        await _context.Database.MigrateAsync();
        if (!_context.Users.Any(u => u.Username == "admin"))
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                List<string> roleNames = ["Admin", "User"];
                List<Role> dbRoles = await _roleRepository.GetRolesByNameAsync(roleNames);
                List<Role> missingRoles = [];
                if (!(dbRoles.Count == 2))
                {
                    List<string> createdNameRoles = [.. dbRoles.Select(r => r.Name)];
                    missingRoles =
                    [
                        .. roleNames
                            .Where(r => !createdNameRoles.Contains(r))
                            .Select(r => new Role { Name = r }),
                    ];
                    await _roleRepository.CreateBulkRolesAsync(missingRoles);
                }
                List<Role> roles = [.. dbRoles, .. missingRoles];
                var adminUser = new User
                {
                    Username = "admin",
                    Email = "example@example.com",
                    Password = _passwordHasher.HashPassword("admin"),
                    Roles = roles,
                };
                await _userRepository.CreateUserAsync(adminUser);
                await _roleRepository.AssignRoleToUserAsync(adminUser, roles, true);
                await transaction.CommitAsync();
                _logger.CustomDebug("successful seeded basic users");
            }
            catch (Exception error)
            {
                await transaction.RollbackAsync();
                _logger.LogError(error, "Error during seed handling");
                throw new Exception("Error during seed handling", error);
            }
            finally
            {
                await transaction.DisposeAsync();
            }
        }
    }
}
