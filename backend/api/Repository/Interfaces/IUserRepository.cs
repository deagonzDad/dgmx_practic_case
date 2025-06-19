using System;
using api.Models;

namespace api.Repository.Interfaces;

public interface IUserRepository
{
    Task<User> GetUserByUsernameAsync(string username);
    Task CreateUserAsync(User user);
    Task<User> GetUserByEmailOrUsernameAsync(string emailOrUsername);
    Task<User> GetUserByEmailAsync(string email);
}
