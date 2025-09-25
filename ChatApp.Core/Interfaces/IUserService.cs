using ChatApp.Core.DTOs;

namespace ChatApp.Core.Interfaces;

public interface IUserService
{
    Task<UserDto?> GetUserAsync(int id);
    Task<IEnumerable<UserDto>> GetUsersAsync();
    Task UpdateUserPresenceAsync(int userId, bool isOnline);
    Task<UserDto> UpdateUserAsync(int userId, UserDto userDto);
}
