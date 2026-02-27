using NotesApi.Models;

namespace NotesApi.Services;

public interface ITokenService
{
    string CreateToken(User user);
    (string Token, DateTime ExpiresAt) CreateRefreshToken();
}
