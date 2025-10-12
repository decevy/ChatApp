using Microsoft.EntityFrameworkCore;
using ChatApp.Core.Entities;
using ChatApp.Core.Extensions;

namespace ChatApp.Core.QueryBuilders;

public class RoomQueryBuilder(IQueryable<Room> query)
{
    private IQueryable<Room> _query = query;

    #region Include properties
    public RoomQueryBuilder WithCreator()
    {
        _query = _query.Include(r => r.Creator);
        return this;
    }

    public RoomQueryBuilder WithMembers(bool includeUsers = false)
    {
        _query = _query
            .Include(r => r.Members)
            .ThenIncludeIf(includeUsers, m => m.User);
        return this;
    }

    public RoomQueryBuilder WithMessages(int? limit = null, bool includeUsers = false)
    {
        _query = (limit.HasValue
                ? _query.Include(r => r.Messages.OrderByDescending(m => m.CreatedAt).Take(limit.Value))
                : _query.Include(r => r.Messages))
            .ThenIncludeIf(includeUsers, m => m.User);
        return this;
    }

    public RoomQueryBuilder WithFullDetails()
    {
        return WithCreator().WithMembers(includeUsers: true).WithMessages(includeUsers: true);
    }
    #endregion

    #region Where properties
    public RoomQueryBuilder WhereId(int id)
    {
        _query = _query.Where(r => r.Id == id);
        return this;
    }

    public RoomQueryBuilder WhereUserIsMember(int userId)
    {
        _query = _query.Where(r => r.Members.Any(m => m.UserId == userId));
        return this;
    }

    public RoomQueryBuilder WhereIsPublic()
    {
        _query = _query.Where(r => !r.IsPrivate);
        return this;
    }

    public RoomQueryBuilder WhereIsPrivate()
    {
        _query = _query.Where(r => r.IsPrivate);
        return this;
    }
    #endregion

    #region Terminal operations

    public async Task<Room> GetByIdAsync(int id)
    {
        return await _query.FirstAsync(r => r.Id == id);
    }
    public async Task<Room?> FindByIdAsync(int id)
    {
        return await _query.FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Room> FirstAsync()
    {
        return await _query.FirstAsync();
    }

    public async Task<Room?> FirstOrDefaultAsync()
    {
        return await _query.FirstOrDefaultAsync();
    }

    public async Task<List<Room>> ToListAsync()
    {
        return await _query.ToListAsync();
    }

    public async Task<int> CountAsync()
    {
        return await _query.CountAsync();
    }

    public async Task<bool> AnyAsync()
    {
        return await _query.AnyAsync();
    }

    public IQueryable<Room> AsQueryable()
    {
        return _query;
    }
    #endregion
}