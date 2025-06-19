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

    public async Task<User> GetUserByEmailOrUsernameAsync(string emailOrUsername)
    {
        User user =
            await _context
                .Users.Where(r => r.Email == emailOrUsername || r.Username == emailOrUsername)
                .FirstOrDefaultAsync() ?? throw new UserNotFoundException();
        return user;
    }

    public async Task<User> GetUserByEmailAsync(string email)
    {
        User user =
            await _context.Users.Where(r => r.Email == email).FirstOrDefaultAsync()
            ?? throw new UserNotFoundException();
        return user;
    }

    public async Task CreateUserAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }
}
