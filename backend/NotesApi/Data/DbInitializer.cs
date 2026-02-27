using System.Data;
using Dapper;

namespace NotesApi.Data;

public sealed class DbInitializer(IDbConnection dbConnection)
{
    public async Task EnsureDatabaseExistsAsync(string databaseName)
    {
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new ArgumentException("Database name is required.", nameof(databaseName));
        }

        var escapedDatabaseName = EscapeIdentifier(databaseName);
        var sql = $"""
                   IF DB_ID(N'{databaseName.Replace("'", "''")}') IS NULL
                   BEGIN
                       EXEC(N'CREATE DATABASE [{escapedDatabaseName}]');
                   END;
                   """;

        await dbConnection.ExecuteAsync(sql);
    }

    public async Task EnsureSchemaAsync()
    {
        const string sql = """
                           IF OBJECT_ID('dbo.Users', 'U') IS NULL
                           BEGIN
                               CREATE TABLE dbo.Users
                               (
                                   Id INT IDENTITY(1,1) PRIMARY KEY,
                                   Name NVARCHAR(100) NOT NULL,
                                   Email NVARCHAR(256) NOT NULL UNIQUE,
                                   PasswordHash NVARCHAR(512) NOT NULL,
                                   CreatedAt DATETIME2 NOT NULL
                               );
                           END;

                           IF OBJECT_ID('dbo.Notes', 'U') IS NULL
                           BEGIN
                               CREATE TABLE dbo.Notes
                               (
                                   Id INT IDENTITY(1,1) PRIMARY KEY,
                                   UserId INT NOT NULL,
                                   Title NVARCHAR(200) NOT NULL,
                                   Content NVARCHAR(MAX) NULL,
                                   CreatedAt DATETIME2 NOT NULL,
                                   UpdatedAt DATETIME2 NOT NULL,
                                   CONSTRAINT FK_Notes_Users_UserId FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
                               );
                           END;

                           IF OBJECT_ID('dbo.RefreshTokens', 'U') IS NULL
                           BEGIN
                               CREATE TABLE dbo.RefreshTokens
                               (
                                   Id INT IDENTITY(1,1) PRIMARY KEY,
                                   UserId INT NOT NULL,
                                   Token NVARCHAR(200) NOT NULL UNIQUE,
                                   ExpiresAt DATETIME2 NOT NULL,
                                   CreatedAt DATETIME2 NOT NULL,
                                   RevokedAt DATETIME2 NULL,
                                   ReplacedByToken NVARCHAR(200) NULL,
                                   CONSTRAINT FK_RefreshTokens_Users_UserId FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
                               );
                           END;

                           IF NOT EXISTS (
                               SELECT 1
                               FROM sys.indexes
                               WHERE name = 'IX_Notes_UserId_UpdatedAt'
                                 AND object_id = OBJECT_ID('dbo.Notes')
                           )
                           BEGIN
                               CREATE INDEX IX_Notes_UserId_UpdatedAt ON dbo.Notes(UserId, UpdatedAt DESC);
                           END;

                           IF NOT EXISTS (
                               SELECT 1
                               FROM sys.indexes
                               WHERE name = 'IX_RefreshTokens_UserId'
                                 AND object_id = OBJECT_ID('dbo.RefreshTokens')
                           )
                           BEGIN
                               CREATE INDEX IX_RefreshTokens_UserId ON dbo.RefreshTokens(UserId);
                           END;
                           """;

        await dbConnection.ExecuteAsync(sql);
    }

    private static string EscapeIdentifier(string value)
    {
        return value.Replace("]", "]]");
    }
}
