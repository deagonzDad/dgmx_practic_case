using api.Data;
using api.Exceptions;
using api.Models;
using api.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace api.Repository;

public class UserRepository(AppDbContext context) : IUserRepository
{
    private readonly AppDbContext _context = context;

    public async Task<User> GetUserByUsernameAsync(string username)
    {
        User user =
            await _context
                .Users.Where(r => r.Username == username)
                .Include(u => u.Roles)
                .FirstOrDefaultAsync() ?? throw new UserNotFoundException();
        return user;
    }
}
