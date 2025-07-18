using System.Linq.Expressions;
using api.Data;
using api.DTO.ResponseDTO;
using api.Exceptions;
using api.Models;
using api.Repository.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace api.Repository;

public class UserRepository(AppDbContext context) : IUserRepository
{
    private readonly string _defaultSortBy = nameof(User.Id);
    private readonly AppDbContext _context = context;
    private readonly HashSet<string> _allowedSortByProperties =
    [
        nameof(User.Id),
        nameof(User.FirstName),
        nameof(User.LastName),
        nameof(User.Username),
        nameof(User.Email),
    ];

    public async Task<User> GetUserByUsernameAsync(string username)
    {
        User user =
            await _context
                .Users.Where(r => r.Username == username)
                .Include(u => u.Roles)
                .FirstOrDefaultAsync() ?? throw new UserNotFoundException(null);
        return user;
    }

    public async Task<User> GetUserByEmailOrUsernameAsync(string emailOrUsername)
    {
        User user =
            await _context
                .Users.Where(r => r.Email == emailOrUsername || r.Username == emailOrUsername)
                .FirstOrDefaultAsync() ?? throw new UserNotFoundException(null);
        return user;
    }

    public async Task<User> GetUserByEmailAsync(string email)
    {
        User user =
            await _context.Users.Where(r => r.Email == email).FirstOrDefaultAsync()
            ?? throw new UserNotFoundException(null);
        return user;
    }

    public async Task CreateUserAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task<(List<User>, int?, int)> GetUsersAsync(FilterParamsDTO filterParamsDTO)
    {
        IQueryable<User> usersQuery = _context.Users.AsNoTracking();
        usersQuery = usersQuery.Where(data => data.IsActive == filterParamsDTO.IsActive);

        if (!string.IsNullOrEmpty(filterParamsDTO.Filter))
        {
            string filterText = $"%{filterParamsDTO.Filter.ToLower()}%";
            usersQuery = usersQuery.Where(data =>
                EF.Functions.Like(data.Id.ToString(), filterText)
                || EF.Functions.Like(data.FirstName.ToLower(), filterText)
                || EF.Functions.Like(data.LastName.ToLower(), filterText)
                || EF.Functions.Like(data.Email.ToLower(), filterText)
                || EF.Functions.Like(data.Username.ToLower(), filterText)
            );
        }

        int totalCount = await usersQuery.CountAsync();

        var sortBy =
            (
                string.IsNullOrWhiteSpace(filterParamsDTO.SortBy)
                || !_allowedSortByProperties.Contains(filterParamsDTO.SortBy)
            )
                ? _defaultSortBy
                : filterParamsDTO.SortBy;

        var isDescending = filterParamsDTO.SortOrder == 0;

        usersQuery = sortBy switch
        {
            nameof(User.FirstName) => isDescending
                ? usersQuery.OrderByDescending(u => u.FirstName)
                : usersQuery.OrderBy(u => u.FirstName),
            nameof(User.LastName) => isDescending
                ? usersQuery.OrderByDescending(u => u.LastName)
                : usersQuery.OrderBy(u => u.LastName),
            nameof(User.Username) => isDescending
                ? usersQuery.OrderByDescending(u => u.Username)
                : usersQuery.OrderBy(u => u.Username),
            nameof(User.Email) => isDescending
                ? usersQuery.OrderByDescending(u => u.Email)
                : usersQuery.OrderBy(u => u.Email),
            _ => isDescending
                ? usersQuery.OrderByDescending(u => u.Id)
                : usersQuery.OrderBy(u => u.Id),
        };

        usersQuery = ((IOrderedQueryable<User>)usersQuery).ThenBy(u => u.Id);

        if (
            !string.IsNullOrWhiteSpace(filterParamsDTO.Cursor)
            && int.TryParse(filterParamsDTO.Cursor, out int cursorId)
        )
        {
            usersQuery = isDescending
                ? usersQuery.Where(u => u.Id < cursorId)
                : usersQuery.Where(u => u.Id > cursorId);
        }

        List<User> userList = await usersQuery.Take(filterParamsDTO.Limit + 1).ToListAsync();

        bool hasMore = userList.Count > filterParamsDTO.Limit;
        if (hasMore)
        {
            userList.RemoveAt(userList.Count - 1);
        }

        int? nextLastId = hasMore ? userList.LastOrDefault()?.Id : null;

        return (userList, nextLastId, totalCount);
    }
}
//  Future Recommendations

//    * Pagination Abstraction: As mentioned before, the pagination logic (especially the cursor
//      encryption/decryption) is still in the controllers. Moving this to the service layer would be a good next
//       step to improve separation of concerns.
//    * Full-Text Search: For the UserRepository, the use of EF.Functions.Like with leading wildcards will not
//      perform well on large datasets. If this is a concern, implementing a proper full-text search solution
//      (like a dedicated search index) would be a major improvement.
//    * Configuration Validation: The JWT configuration in ServiceExtensions.cs could be made more robust by
//      adding validation to ensure that the required configuration values are present at application startup.
