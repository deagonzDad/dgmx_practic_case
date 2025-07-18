using api.DTO.ResponseDTO;
using api.Models;

namespace api.Repository.Interfaces;

public interface IUserRepository
{
    Task<User> GetUserByUsernameAsync(string username);
    Task CreateUserAsync(User user);
    Task<User> GetUserByEmailOrUsernameAsync(string emailOrUsername);
    Task<User> GetUserByEmailAsync(string email);
    Task<(List<User>, int?, int)> GetUsersAsync(FilterParamsDTO filterParamsDTO);
    Task<bool> UsernameOrEmailExistsAsync(string email, string username);
}
