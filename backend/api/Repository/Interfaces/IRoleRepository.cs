using System;
using api.Models;

namespace api.Repository.Interfaces;

public interface IRoleRepository
{
    Task CreateRolAsync();
    Task<(bool, List<Role>)> ValidateRolesExistByIdAsync(List<int> rolesIds);
    Task AssignRoleToUserAsync(User user, List<Role> roles, bool isNew);
    Task<List<Role>> GetRolesByNameAsync(List<string> roleNames);
    Task CreateBulkRolesAsync(List<Role> listRoles);
}
