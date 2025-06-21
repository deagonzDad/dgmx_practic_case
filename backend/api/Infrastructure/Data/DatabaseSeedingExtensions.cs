using System.Collections.Immutable;
using api.Common;
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
        const string DefaultUserName = "admin";
        const string DefaultPassword = "Admin_123";
        const string DefaultEmail = "example@example.com";
        ImmutableList<string> roleNames = [AppRoles.Admin, AppRoles.User];
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
            bool existedAdminUsers = await _context.Users.AnyAsync(ur =>
                ur.Roles.Any(r => r.Name.Equals(AppRoles.Admin))
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
                        Email = DefaultEmail,
                        Password = _passwordHasher.HashPassword(DefaultPassword),
                        Roles = roles,
                    };
                    await _userRepository.CreateUserAsync(adminUser);
                    existedAdminUser = adminUser;
                }
                else
                {
                    await _roleRepository.AssignRoleToUserAsync(existedAdminUser, roles, true);
                }
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
