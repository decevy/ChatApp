using ChatApp.Core.Dtos;
using ChatApp.Core.Dtos.Requests;

namespace ChatApp.Core.Interfaces;

public interface IUserService
{
    Task<UserDto?> GetUserAsync(int id);
    Task<IEnumerable<UserDto>> GetUsersAsync();
    Task<IEnumerable<UserDto>> SearchUsersAsync(string query);
    Task<UserDto> UpdateUserAsync(int userId, UpdateUserRequest request);
    Task UpdateUserPresenceAsync(int userId, bool isOnline);
}