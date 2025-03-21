using System;
using api.Models;

namespace api.Repository.Interfaces;

public interface IRoleRepository
{
    Task CreateRolAsync();
    Task<(bool, List<Role>)> ValidateRolesExistAsync(List<int> rolesIds);
    Task AssignRoleToUserAsync(int userId, List<Role> roleId);
}
