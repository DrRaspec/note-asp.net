using System.Data;
using Dapper;
using NotesApi.Models;

namespace NotesApi.Repositories;

public sealed class RefreshTokenRepository(IDbConnection dbConnection) : IRefreshTokenRepository
{
    public async Task<int> CreateAsync(RefreshToken refreshToken)
    {
        const string sql = """
                           INSERT INTO dbo.RefreshTokens (UserId, Token, ExpiresAt, CreatedAt, RevokedAt, ReplacedByToken)
                           VALUES (@UserId, @Token, @ExpiresAt, @CreatedAt, @RevokedAt, @ReplacedByToken);
                           SELECT CAST(SCOPE_IDENTITY() AS INT);
                           """;

        return await dbConnection.ExecuteScalarAsync<int>(sql, refreshToken);
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        const string sql = """
                           SELECT Id, UserId, Token, ExpiresAt, CreatedAt, RevokedAt, ReplacedByToken
                           FROM dbo.RefreshTokens
                           WHERE Token = @Token;
                           """;

        return await dbConnection.QueryFirstOrDefaultAsync<RefreshToken>(sql, new { Token = token });
    }

    public async Task<bool> RevokeAsync(int id, DateTime revokedAt, string? replacedByToken)
    {
        const string sql = """
                           UPDATE dbo.RefreshTokens
                           SET RevokedAt = @RevokedAt,
                               ReplacedByToken = @ReplacedByToken
                           WHERE Id = @Id
                             AND RevokedAt IS NULL;
                           """;

        var affected = await dbConnection.ExecuteAsync(sql, new { Id = id, RevokedAt = revokedAt, ReplacedByToken = replacedByToken });
        return affected == 1;
    }

    public async Task<int> RevokeAllForUserAsync(int userId, DateTime revokedAt)
    {
        const string sql = """
                           UPDATE dbo.RefreshTokens
                           SET RevokedAt = @RevokedAt
                           WHERE UserId = @UserId
                             AND RevokedAt IS NULL;
                           """;

        return await dbConnection.ExecuteAsync(sql, new { UserId = userId, RevokedAt = revokedAt });
    }
}
