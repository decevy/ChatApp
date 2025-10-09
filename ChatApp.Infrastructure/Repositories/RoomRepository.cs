using Microsoft.EntityFrameworkCore;
using ChatApp.Core.Entities;
using ChatApp.Core.Extensions;
using ChatApp.Core.Interfaces;
using ChatApp.Infrastructure.Data;

namespace ChatApp.Infrastructure.Repositories;

public class RoomRepository(ChatDbContext context) : IRoomRepository
{
    #region Room CRUD operations
    public async Task<Room> CreateAsync(Room room)
    {
        context.Rooms.Add(room);
        await context.SaveChangesAsync();
        return room;
    }

    public async Task<Room?> GetByIdAsync(int id)
    {
        return await context.Rooms
            .Include(r => r.Creator)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Room?> GetByIdWithMembersAsync(int id)
    {
        return await context.Rooms
            .Include(r => r.Creator)
            .Include(r => r.Members)
                .ThenInclude(m => m.User)
            .Include(r => r.Messages.OrderByDescending(m => m.CreatedAt).Take(50))
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<Room>> GetUserRoomsAsync(int userId,
        bool includeCreator = false,
        bool includeMembers = false, bool includeMemberUsers = false,
        bool includeMessages = false, bool includeMessageUsers = false)
    {
        return await context.Rooms
            .IncludeIf(includeCreator, r => r.Creator)
            .IncludeIf(includeMembers || includeMemberUsers, r => r.Members)
                .ThenIncludeIf(includeMemberUsers, m => m.User)
            .IncludeIf(includeMessages || includeMessageUsers, r => r.Messages)
                .ThenIncludeIf(includeMessageUsers, m => m.User)
            .Where(r => r.Members.Any(m => m.UserId == userId))
            .ToListAsync();
    }

    public async Task<IEnumerable<Room>> GetPublicRoomsAsync()
    {
        return await context.Rooms
            .Include(r => r.Creator)
            .Where(r => !r.IsPrivate)
            .ToListAsync();
    }

    public async Task UpdateAsync(Room room)
    {
        context.Entry(room).State = EntityState.Modified;
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var room = await context.Rooms.FindAsync(id);
        if (room != null)
        {
            context.Rooms.Remove(room);
            await context.SaveChangesAsync();
        }
    }
    #endregion

    #region Room Member operations
    public async Task<RoomMember?> GetRoomMemberAsync(int roomId, int userId)
    {
        return await context.RoomMembers
            .Include(rm => rm.User)
            .FirstOrDefaultAsync(rm => rm.RoomId == roomId && rm.UserId == userId);
    }

    public async Task AddRoomMemberAsync(RoomMember member)
    {
        context.RoomMembers.Add(member);
        await context.SaveChangesAsync();
    }

    public async Task RemoveRoomMemberAsync(int roomId, int userId)
    {
        var member = await GetRoomMemberAsync(roomId, userId);
        if (member != null)
        {
            context.RoomMembers.Remove(member);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> IsUserMemberAsync(int roomId, int userId)
    {
        return await context.RoomMembers
            .AnyAsync(rm => rm.RoomId == roomId && rm.UserId == userId);
    }

    public async Task<bool> IsUserRoomAdminAsync(int roomId, int userId)
    {
        return await context.RoomMembers
            .AnyAsync(
                rm => rm.RoomId == roomId && rm.UserId == userId 
                && rm.Role == RoomRole.Admin);
    }
    #endregion

    #region Message operations

    public async Task<Message?> GetLastMessageInRoomAsync(int roomId)
    {
        return await context.Messages
            .Include(m => m.User)
            .Where(m => m.RoomId == roomId)
            .OrderByDescending(m => m.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<(IEnumerable<Message> messages, int totalCount)> GetRoomMessagesPagedAsync(int roomId, int page, int pageSize)
    {
        var query = context.Messages
            .Include(m => m.User)
            .Include(m => m.Reactions)
                .ThenInclude(r => r.User)
            .Where(m => m.RoomId == roomId);

        var totalCount = await query.CountAsync();
        
        var messages = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (messages, totalCount);
    }
    #endregion
}
