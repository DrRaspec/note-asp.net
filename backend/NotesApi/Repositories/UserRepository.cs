using System.Data;
using Dapper;
using NotesApi.Models;

namespace NotesApi.Repositories;

public sealed class UserRepository(IDbConnection dbConnection) : IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email)
    {
        const string sql = """
                           SELECT Id, Name, Email, PasswordHash, CreatedAt
                           FROM dbo.Users
                           WHERE Email = @Email;
                           """;

        return await dbConnection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        const string sql = """
                           SELECT Id, Name, Email, PasswordHash, CreatedAt
                           FROM dbo.Users
                           WHERE Id = @Id;
                           """;

        return await dbConnection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
    }

    public async Task<int> CreateAsync(User user)
    {
        const string sql = """
                           INSERT INTO dbo.Users (Name, Email, PasswordHash, CreatedAt)
                           VALUES (@Name, @Email, @PasswordHash, @CreatedAt);
                           SELECT CAST(SCOPE_IDENTITY() AS INT);
                           """;

        return await dbConnection.ExecuteScalarAsync<int>(sql, user);
    }
}
