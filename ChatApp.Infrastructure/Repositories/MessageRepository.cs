using Microsoft.EntityFrameworkCore;
using ChatApp.Core.Entities;
using ChatApp.Core.Interfaces;
using ChatApp.Core.QueryBuilders;
using ChatApp.Infrastructure.Data;

namespace ChatApp.Infrastructure.Repositories;

public class MessageRepository(ChatDbContext context) : IMessageRepository
{
    public MessageQueryBuilder Query()
    {
        return new MessageQueryBuilder(context.Messages.AsQueryable());
    }

    public async Task<Message> CreateAsync(Message message)
    {
        context.Messages.Add(message);
        await context.SaveChangesAsync();
        
        // Return the message with includes
        return await Query()
            .WithFullDetails()
            .GetByIdAsync(message.Id) ?? message;
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
        return await Query()
            .WithUser()
            .WhereRoomId(roomId)
            .OrderByNewest()
            .FirstOrDefaultAsync();
    }
}
