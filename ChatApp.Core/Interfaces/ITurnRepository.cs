using ChatApp.Core.Entities;
using ChatApp.Core.QueryBuilders;

namespace ChatApp.Core.Interfaces;

public interface ITurnRepository
{
    TurnQueryBuilder Query();
    Task<Turn?> GetByIdAsync(int id);
    Task<Turn> CreateAsync(Turn turn);
    Task UpdateAsync(Turn turn);
    Task DeleteAsync(int id);

    Task<Turn?> GetLastTurnInStoryAsync(int storyId);
}
