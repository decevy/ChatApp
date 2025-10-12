using Microsoft.EntityFrameworkCore;
using ChatApp.Core.Entities;
using ChatApp.Core.Interfaces;
using ChatApp.Core.QueryBuilders;
using ChatApp.Infrastructure.Data;

namespace ChatApp.Infrastructure.Repositories;

public class RoomRepository(ChatDbContext context) : IRoomRepository
{
    #region Rooms
    public RoomQueryBuilder Query()
    {
        return new RoomQueryBuilder(context.Rooms.AsQueryable());
    }

    public async Task<Room> GetByIdAsync(int id)
    {
        return await Query().GetByIdAsync(id);
    }

    public async Task<Room?> FindByIdAsync(int id)
    {
        return await Query().FindByIdAsync(id);
    }

    public async Task<Room> CreateAsync(Room room)
    {
        context.Rooms.Add(room);
        await context.SaveChangesAsync();
        
        return await Query()
            .WithFullDetails()
            .GetByIdAsync(room.Id);
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

    public async Task<bool> ExistsAsync(int id)
    {
        return await context.Rooms.AnyAsync(r => r.Id == id);
    }
    #endregion

    #region Room Members
    public RoomMemberQueryBuilder QueryRoomMembers()
    {
        return new RoomMemberQueryBuilder(context.RoomMembers.AsQueryable());
    }

    public async Task<RoomMember?> FindRoomMemberAsync(int roomId, int userId)
    {
        return await QueryRoomMembers()
            .WhereRoomAndUser(roomId, userId)
            .WithUser()
            .FirstOrDefaultAsync();
    }

    public async Task AddRoomMemberAsync(RoomMember member)
    {
        context.RoomMembers.Add(member);
        await context.SaveChangesAsync();
    }

    public async Task RemoveRoomMemberAsync(int roomId, int userId)
    {
        var member = await FindRoomMemberAsync(roomId, userId);
        if (member != null)
        {
            context.RoomMembers.Remove(member);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> IsUserMemberAsync(int roomId, int userId)
    {
        return await QueryRoomMembers()
            .WhereRoomAndUser(roomId, userId)
            .AnyAsync();
    }

    public async Task<bool> IsUserRoomAdminAsync(int roomId, int userId)
    {
        return await QueryRoomMembers()
            .WhereRoomAndUser(roomId, userId)
            .WhereAdmin()
            .AnyAsync();
    }
    #endregion
}
