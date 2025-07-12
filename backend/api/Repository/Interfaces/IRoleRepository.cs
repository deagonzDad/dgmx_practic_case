using api.Models;

namespace api.Repository.Interfaces;

public interface IRoleRepository
{
    Task CreateRolAsync();
    Task<List<Role>> GetRolesByIdAsync(List<int> rolesIds);
    Task AssignRoleToUserAsync(User user, List<Role> roles, bool isNew);
    Task<List<Role>> GetRolesByNameAsync(ICollection<string> roleNames);
    Task CreateBulkRolesAsync(List<Role> listRoles);
}
