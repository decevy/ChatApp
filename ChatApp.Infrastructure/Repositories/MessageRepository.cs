using Microsoft.EntityFrameworkCore;
using ChatApp.Core.Entities;
using ChatApp.Core.Interfaces;
using ChatApp.Infrastructure.Data;

namespace ChatApp.Infrastructure.Repositories;

public class MessageRepository(ChatDbContext context) : IMessageRepository
{
    public async Task<Message?> GetByIdAsync(int id)
    {
        return await context.Messages
            .Include(m => m.User)
            .Include(m => m.Reactions)
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<Message>> GetRoomMessagesAsync(int roomId, int page = 1, int pageSize = 50)
    {
        return await context.Messages
            .Include(m => m.User)
            .Include(m => m.Reactions)
                .ThenInclude(r => r.User)
            .Where(m => m.RoomId == roomId)
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Message> CreateAsync(Message message)
    {
        context.Messages.Add(message);
        await context.SaveChangesAsync();
        
        // Return the message with includes
        return await GetByIdAsync(message.Id) ?? message;
    }

    public async Task UpdateAsync(Message message)
    {
        context.Entry(message).State = EntityState.Modified;
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var message = await context.Messages.FindAsync(id);
        if (message != null)
        {
            context.Messages.Remove(message);
            await context.SaveChangesAsync();
        }
    }

    public async Task<Message?> GetLastMessageInRoomAsync(int roomId)
    {
        return await context.Messages
            .Include(m => m.User)
            .Where(m => m.RoomId == roomId)
            .OrderByDescending(m => m.CreatedAt)
            .FirstOrDefaultAsync();
    }
}
