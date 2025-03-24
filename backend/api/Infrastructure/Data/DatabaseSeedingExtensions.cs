using System;
using System.Collections.Immutable;
using api.Data;
using api.Helpers;
using api.Helpers.Instances;
using api.Models;
using api.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;

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
        const string DefaultUserName = "admin";
        ImmutableList<string> roleNames = ["Admin", "User"];
        await _context.Database.MigrateAsync();
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
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
            var existedAdminUsers = await _context.UserRoles.AnyAsync(ur =>
                ur.Role.Name == "Admin"
            );

            if (!existedAdminUsers)
            {
                User? existedAdminUser = await _context.Users.FirstOrDefaultAsync(r =>
                    r.Username == DefaultUserName
                );
                if (existedAdminUser == null)
                {
                    User adminUser = new()
                    {
                        Username = DefaultUserName,
                        Email = "example@example.com",
                        Password = _passwordHasher.HashPassword(DefaultUserName),
                        Roles = roles,
                    };
                    await _userRepository.CreateUserAsync(adminUser);
                    existedAdminUser = adminUser;
                }
                await _roleRepository.AssignRoleToUserAsync(existedAdminUser, roles, true);
            }
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
    // }
}
