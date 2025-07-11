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
        try
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            var innerException = ex.InnerException;
            if (innerException is SqliteException sqliteEx && sqliteEx.SqliteErrorCode == 19)
                throw new AlreadyExistException(ex);
            throw new UpdateException(ex);
        }
    }

    public async Task<(List<User>, int?, int)> GetUsersAsync(FilterParamsDTO filterParamsDTO)
    {
        try
        {
            IQueryable<User> usersQuery = _context.Users.AsQueryable().AsNoTracking();
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
            bool hasSortBy =
                !string.IsNullOrWhiteSpace(filterParamsDTO.SortBy)
                && _allowedSortByProperties.Contains(filterParamsDTO.SortBy);
            int sortOrder = filterParamsDTO.SortOrder;
            if (hasSortBy)
            {
                ParameterExpression parameter = Expression.Parameter(typeof(User), "u");
                MemberExpression expression = Expression.Property(
                    parameter,
                    filterParamsDTO.SortBy!
                );
                Expression<Func<User, object>> lambda = Expression.Lambda<Func<User, object>>(
                    Expression.Convert(expression, typeof(object)),
                    parameter
                );

                if (sortOrder == 1)
                    usersQuery = usersQuery.OrderBy(lambda);
                else
                    usersQuery = usersQuery.OrderByDescending(lambda);
            }
            else
            {
                if (sortOrder == 1)
                    usersQuery = usersQuery.OrderBy(u => u.Id);
                else
                    usersQuery = usersQuery.OrderByDescending(u => u.Id);
                filterParamsDTO.SortBy = _defaultSortBy;
            }
            if (
                hasSortBy
                && !filterParamsDTO.SortBy!.Equals(
                    nameof(User.Id),
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                if (sortOrder == 1)
                    usersQuery = ((IOrderedQueryable<User>)usersQuery).ThenBy(u => u.Id);
                else
                    usersQuery = ((IOrderedQueryable<User>)usersQuery).ThenByDescending(u => u.Id);
            }
            int totalCount = await usersQuery.CountAsync();
            if (
                !string.IsNullOrWhiteSpace(filterParamsDTO.Cursor)
                && int.TryParse(filterParamsDTO.Cursor, out int cursorId)
            )
            {
                if (sortOrder == 1)
                    usersQuery = usersQuery.Where(u => u.Id > cursorId);
                else
                    usersQuery = usersQuery.Where(u => u.Id < cursorId);
            }
            List<User> userList = await usersQuery.Take(filterParamsDTO.Limit + 1).ToListAsync();
            bool hasMore = userList.Count > filterParamsDTO.Limit;
            if (hasMore)
                userList.RemoveAt(userList.Count - 1);
            int? nextLastId = hasMore ? userList[^1].Id : null;
            return (userList, nextLastId, totalCount);
        }
        catch (DbUpdateException ex)
        {
            throw new UpdateException(ex);
        }
        throw new NotImplementedException();
    }
}
