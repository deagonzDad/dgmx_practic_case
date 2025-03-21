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

    public async Task<(bool, List<Role>)> ValidateRolesExistAsync(List<int> rolesIds)
    {
        int userId = 1;
        List<Role> existingRoleIds = await _context
            .Roles.Where(r => rolesIds.Contains(r.Id))
            .ToListAsync();
        //create validator if the user is new don't get all the roles assigned to the user
        //if the user is old then get the user with the roles to validate if the roles are repeated or not
        //TODO: Investigate if I can do a bulk create and only ignore the relationship if the role already exists
        // User roleAssignedToUser = await _context.Users.Include(u=>u.Roles).FirstOrDefaultAsync(u=>u.Id == userId)
        bool allRolesExist = existingRoleIds.Count == rolesIds.Count;
        List<Role> filteredRoles = await existingRoleIds.Where(r => !roleIds)
        return (allRolesExist, existingRoleIds);
    }

    public async Task AssignRoleToUserAsync(List<int> userIds, int roleId)
    {

    }
}
