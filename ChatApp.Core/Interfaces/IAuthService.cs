using ChatApp.Core.Dtos.Requests;
using ChatApp.Core.Dtos.Responses;

namespace ChatApp.Core.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> RegisterAsync(RegisterRequest request);
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<LoginResponse> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeTokenAsync(int userId);
    string GenerateJwtToken(int userId, string email, string username);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}