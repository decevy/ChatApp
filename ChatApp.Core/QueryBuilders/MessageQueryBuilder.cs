using Microsoft.EntityFrameworkCore;
using ChatApp.Core.Entities;
using ChatApp.Core.Extensions;

namespace ChatApp.Core.QueryBuilders;

public class MessageQueryBuilder(IQueryable<Message> query)
{
    private IQueryable<Message> _query = query;

    #region Include properties
    public MessageQueryBuilder WithUser()
    {
        _query = _query.Include(m => m.User);
        return this;
    }

    public MessageQueryBuilder WithRoom()
    {
        _query = _query.Include(m => m.Room);
        return this;
    }

    public MessageQueryBuilder WithReactions(bool includeUsers = false)
    {
        _query = _query
            .Include(m => m.Reactions)
            .ThenIncludeIf(includeUsers, r => r.User);
        return this;
    }
    
    public MessageQueryBuilder WithFullDetails()
    {
        return WithUser().WithRoom().WithReactions(includeUsers: true);
    }
    #endregion

    #region Where properties
    public MessageQueryBuilder WhereId(int id)
    {
        _query = _query.Where(m => m.Id == id);
        return this;
    }

    public MessageQueryBuilder WhereRoomId(int roomId)
    {
        _query = _query.Where(m => m.RoomId == roomId);
        return this;
    }

    public MessageQueryBuilder WhereUserId(int userId)
    {
        _query = _query.Where(m => m.UserId == userId);
        return this;
    }

    public MessageQueryBuilder WhereType(MessageType type)
    {
        _query = _query.Where(m => m.Type == type);
        return this;
    }

    public MessageQueryBuilder WhereEdited()
    {
        _query = _query.Where(m => m.EditedAt != null);
        return this;
    }

    public MessageQueryBuilder WhereHasAttachment()
    {
        _query = _query.Where(m => m.AttachmentUrl != null);
        return this;
    }

    public MessageQueryBuilder WhereCreatedAfter(DateTime date)
    {
        _query = _query.Where(m => m.CreatedAt > date);
        return this;
    }

    public MessageQueryBuilder WhereCreatedBefore(DateTime date)
    {
        _query = _query.Where(m => m.CreatedAt < date);
        return this;
    }
    #endregion

    #region Order properties
    public MessageQueryBuilder OrderByNewest()
    {
        _query = _query.OrderByDescending(m => m.CreatedAt);
        return this;
    }

    public MessageQueryBuilder OrderByOldest()
    {
        _query = _query.OrderBy(m => m.CreatedAt);
        return this;
    }
    #endregion

    #region Paginate properties
    public MessageQueryBuilder Paginate(int page, int pageSize)
    {
        _query = _query
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
        return this;
    }

    public MessageQueryBuilder Take(int count)
    {
        _query = _query.Take(count);
        return this;
    }

    public MessageQueryBuilder Skip(int count)
    {
        _query = _query.Skip(count);
        return this;
    }
    #endregion

    #region Terminal operations
    public async Task<Message> GetByIdAsync(int id)
    {
        return await _query.FirstAsync(m => m.Id == id);
    }

    public async Task<Message?> FindByIdAsync(int id)
    {
        return await _query.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Message> FirstAsync()
    {
        return await _query.FirstAsync();
    }

    public async Task<Message?> FirstOrDefaultAsync()
    {
        return await _query.FirstOrDefaultAsync();
    }

    public async Task<List<Message>> ToListAsync()
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

    public async Task<(List<Message> messages, int totalCount)> ToPagedListAsync(int page, int pageSize)
    {
        var totalCount = await _query.CountAsync();
        var messages = await _query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return (messages, totalCount);
    }
    
    public IQueryable<Message> AsQueryable()
    {
        return _query;
    }
    #endregion

}

