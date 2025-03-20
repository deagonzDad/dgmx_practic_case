using System;
using api.Models;

namespace api.Repository.Interfaces;

public interface IUserRepository
{
    Task<User> GetUserByUsernameAsync(string username);
}
