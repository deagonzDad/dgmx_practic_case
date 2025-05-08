using api.Data;
using api.Exceptions;
using api.Models;
using api.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace api.Repository;

public class RoleRepository(AppDbContext context) : IRoleRepository
{
    private readonly AppDbContext _context = context;

    public Task CreateRolAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<(bool, List<Role>)> ValidateRolesExistByIdAsync(List<int> rolesIds)
    {
        List<Role> existingRoleIds = await _context
            .Roles.Where(r => rolesIds.Contains(r.Id) || r.Name == "User")
            .ToListAsync();
        bool allRolesExist = existingRoleIds.Count == rolesIds.Count;
        return (allRolesExist, existingRoleIds);
    }

    public async Task<List<Role>> GetRolesByNameAsync(ICollection<string> roleNames)
    {
        List<Role> rolesExisted = await _context
            .Roles.Where(r => roleNames.Contains(r.Name))
            .ToListAsync();
        return rolesExisted;
    }

    private async Task<List<Role>> GetRolesAssignToUserAsync(User user)
    {
        List<Role> rolesAssignToUser = await _context
            .UserRoles.Where(u => u.UserId == user.Id)
            .Include(ur => ur.Role)
            .Select(ur => ur.Role)
            .Distinct()
            .ToListAsync();
        return rolesAssignToUser;
    }

    public async Task AssignRoleToUserAsync(User user, List<Role> roles, bool isNew)
    {
        List<Role> rolesToAssign = roles;

        if (!isNew)
        {
            List<Role> rolesAssignToUser = await GetRolesAssignToUserAsync(user);
            rolesToAssign = [.. roles.Where(r => !rolesAssignToUser.Contains(r))];
        }
        List<UserRole> relationUserRole = [];
        foreach (Role role in rolesToAssign)
        {
            relationUserRole.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
            //user.UserRoles.Add(new UserRole { User = user, Role = role });
        }

        await _context.UserRoles.AddRangeAsync(relationUserRole);
        await _context.SaveChangesAsync();
    }

    public async Task CreateBulkRolesAsync(List<Role> listRoles)
    {
        await _context.AddRangeAsync(listRoles);
        await _context.SaveChangesAsync();
    }

    // public async Task CreateRoleAsync(Role role) { }
}
