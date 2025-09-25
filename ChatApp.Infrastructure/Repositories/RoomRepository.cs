using Microsoft.EntityFrameworkCore;
using ChatApp.Core.Entities;
using ChatApp.Core.Interfaces;
using ChatApp.Infrastructure.Data;

namespace ChatApp.Infrastructure.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly ChatDbContext _context;

    public RoomRepository(ChatDbContext context)
    {
        _context = context;
    }

    public async Task<Room?> GetByIdAsync(int id)
    {
        return await _context.Rooms
            .Include(r => r.Creator)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Room?> GetByIdWithMembersAsync(int id)
    {
        return await _context.Rooms
            .Include(r => r.Creator)
            .Include(r => r.Members)
                .ThenInclude(m => m.User)
            .Include(r => r.Messages.OrderByDescending(m => m.CreatedAt).Take(50))
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<Room>> GetUserRoomsAsync(int userId)
    {
        return await _context.Rooms
            .Include(r => r.Creator)
            .Where(r => r.Members.Any(m => m.UserId == userId))
            .ToListAsync();
    }

    public async Task<IEnumerable<Room>> GetPublicRoomsAsync()
    {
        return await _context.Rooms
            .Include(r => r.Creator)
            .Where(r => !r.IsPrivate)
            .ToListAsync();
    }

    public async Task<Room> CreateAsync(Room room)
    {
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
        return room;
    }

    public async Task UpdateAsync(Room room)
    {
        _context.Entry(room).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room != null)
        {
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> IsUserMemberAsync(int roomId, int userId)
    {
        return await _context.RoomMembers
            .AnyAsync(rm => rm.RoomId == roomId && rm.UserId == userId);
    }
}
