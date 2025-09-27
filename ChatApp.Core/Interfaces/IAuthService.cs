using ChatApp.Core.Dtos;
using ChatApp.Core.Dtos.Requests;

namespace ChatApp.Core.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> RegisterAsync(RegisterRequest request);
    Task<LoginResponse> LoginAsync(LoginRequest request);
    string GenerateJwtToken(int userId, string email, string username);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}
