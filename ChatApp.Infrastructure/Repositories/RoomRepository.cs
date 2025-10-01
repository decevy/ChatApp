using Microsoft.EntityFrameworkCore;
using ChatApp.Core.Entities;
using ChatApp.Core.Interfaces;
using ChatApp.Infrastructure.Data;

namespace ChatApp.Infrastructure.Repositories;

public class RoomRepository(ChatDbContext context) : IRoomRepository
{
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

    public async Task<IEnumerable<Room>> GetUserRoomsAsync(int userId)
    {
        return await context.Rooms
            .Include(r => r.Creator)
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

    public async Task<Room> CreateAsync(Room room)
    {
        context.Rooms.Add(room);
        await context.SaveChangesAsync();
        return room;
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

    public async Task<bool> IsUserMemberAsync(int roomId, int userId)
    {
        return await context.RoomMembers
            .AnyAsync(rm => rm.RoomId == roomId && rm.UserId == userId);
    }
}
