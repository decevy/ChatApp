using Microsoft.EntityFrameworkCore;
using ChatApp.Core.Entities;

namespace ChatApp.Core.QueryBuilders;

public class RoomMemberQueryBuilder(IQueryable<RoomMember> query)
{
    private IQueryable<RoomMember> _query = query;

    #region Include properties
    public RoomMemberQueryBuilder WithUser()
    {
        _query = _query.Include(rm => rm.User);
        return this;
    }

    public RoomMemberQueryBuilder WithRoom()
    {
        _query = _query.Include(rm => rm.Room);
        return this;
    }

    public RoomMemberQueryBuilder WithFullDetails()
    {
        return WithUser().WithRoom();
    }
    #endregion

    #region Where properties
    public RoomMemberQueryBuilder WhereId(int id)
    {
        _query = _query.Where(rm => rm.Id == id);
        return this;
    }

    public RoomMemberQueryBuilder WhereRoomId(int roomId)
    {
        _query = _query.Where(rm => rm.RoomId == roomId);
        return this;
    }

    public RoomMemberQueryBuilder WhereUserId(int userId)
    {
        _query = _query.Where(rm => rm.UserId == userId);
        return this;
    }

    public RoomMemberQueryBuilder WhereRoomAndUser(int roomId, int userId)
    {
        _query = _query.Where(rm => rm.RoomId == roomId && rm.UserId == userId);
        return this;
    }

    public RoomMemberQueryBuilder WhereRole(RoomRole role)
    {
        _query = _query.Where(rm => rm.Role == role);
        return this;
    }

    public RoomMemberQueryBuilder WhereAdmin()
    {
        _query = _query.Where(rm => rm.Role == RoomRole.Admin);
        return this;
    }

    public RoomMemberQueryBuilder WhereMember()
    {
        _query = _query.Where(rm => rm.Role == RoomRole.Member);
        return this;
    }

    public RoomMemberQueryBuilder WhereJoinedAfter(DateTime date)
    {
        _query = _query.Where(rm => rm.JoinedAt > date);
        return this;
    }

    public RoomMemberQueryBuilder WhereJoinedBefore(DateTime date)
    {
        _query = _query.Where(rm => rm.JoinedAt < date);
        return this;
    }
    #endregion

    #region Order properties
    public RoomMemberQueryBuilder OrderByJoinedAt()
    {
        _query = _query.OrderBy(rm => rm.JoinedAt);
        return this;
    }

    public RoomMemberQueryBuilder OrderByJoinedAtDescending()
    {
        _query = _query.OrderByDescending(rm => rm.JoinedAt);
        return this;
    }

    public RoomMemberQueryBuilder OrderByRole()
    {
        _query = _query.OrderBy(rm => rm.Role);
        return this;
    }
    #endregion

    #region Paginate properties
    public RoomMemberQueryBuilder Paginate(int page, int pageSize)
    {
        _query = _query
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
        return this;
    }

    public RoomMemberQueryBuilder Take(int count)
    {
        _query = _query.Take(count);
        return this;
    }

    public RoomMemberQueryBuilder Skip(int count)
    {
        _query = _query.Skip(count);
        return this;
    }
    #endregion

    #region Terminal operations
    public async Task<RoomMember> GetByIdAsync(int id)
    {
        return await _query.FirstAsync(rm => rm.Id == id);
    }

    public async Task<RoomMember?> FindByIdAsync(int id)
    {
        return await _query.FirstOrDefaultAsync(rm => rm.Id == id);
    }

    public async Task<RoomMember> FirstAsync()
    {
        return await _query.FirstAsync();
    }

    public async Task<RoomMember?> FirstOrDefaultAsync()
    {
        return await _query.FirstOrDefaultAsync();
    }

    public async Task<List<RoomMember>> ToListAsync()
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

    public async Task<(List<RoomMember> members, int totalCount)> ToPagedListAsync(int page, int pageSize)
    {
        var totalCount = await _query.CountAsync();
        var members = await _query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return (members, totalCount);
    }
    
    public IQueryable<RoomMember> AsQueryable()
    {
        return _query;
    }
    #endregion

}

