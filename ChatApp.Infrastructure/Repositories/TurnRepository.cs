using Microsoft.EntityFrameworkCore;
using ChatApp.Core.Entities;
using ChatApp.Core.Interfaces;
using ChatApp.Core.QueryBuilders;
using ChatApp.Infrastructure.Data;

namespace ChatApp.Infrastructure.Repositories;

public class TurnRepository(ChatDbContext context) : ITurnRepository
{
    public TurnQueryBuilder Query()
    {
        return new TurnQueryBuilder(context.Turns.AsQueryable());
    }

    public async Task<Turn?> GetByIdAsync(int id)
    {
        return await Query().FindByIdAsync(id);
    }

    public async Task<Turn> CreateAsync(Turn turn)
    {
        context.Turns.Add(turn);
        await context.SaveChangesAsync();

        return await Query()
            .WithFullDetails()
            .FindByIdAsync(turn.Id) ?? turn;
    }

    public async Task UpdateAsync(Turn turn)
    {
        context.Entry(turn).State = EntityState.Modified;
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var turn = await context.Turns.FindAsync(id);
        if (turn != null)
        {
            context.Turns.Remove(turn);
            await context.SaveChangesAsync();
        }
    }

    public async Task<Turn?> GetLastTurnInStoryAsync(int storyId)
    {
        return await Query()
            .WithUser()
            .WhereStoryId(storyId)
            .OrderByNewest()
            .FirstOrDefaultAsync();
    }
}
