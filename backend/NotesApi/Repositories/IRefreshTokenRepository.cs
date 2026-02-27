using NotesApi.Models;

namespace NotesApi.Repositories;

public interface IRefreshTokenRepository
{
    Task<int> CreateAsync(RefreshToken refreshToken);
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<bool> RevokeAsync(int id, DateTime revokedAt, string? replacedByToken);
    Task<int> RevokeAllForUserAsync(int userId, DateTime revokedAt);
}
