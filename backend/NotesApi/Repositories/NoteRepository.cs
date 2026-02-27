using System.Data;
using Dapper;
using NotesApi.Contracts;
using NotesApi.Models;

namespace NotesApi.Repositories;

public sealed class NoteRepository(IDbConnection dbConnection) : INoteRepository
{
    public async Task<PagedResult<Note>> GetPageAsync(
        int userId,
        string? search,
        string? sortBy,
        string? sortDir,
        int page,
        int pageSize)
    {
        var normalizedSortBy = NormalizeSortBy(sortBy);
        var normalizedSortDir = NormalizeSortDir(sortDir);
        var offset = (page - 1) * pageSize;

        var countSql = """
                       SELECT COUNT(1)
                       FROM dbo.Notes
                       WHERE UserId = @UserId
                         AND (@Search IS NULL OR Title LIKE '%' + @Search + '%' OR Content LIKE '%' + @Search + '%');
                       """;

        var notesSql = $"""
                        SELECT Id, UserId, Title, Content, CreatedAt, UpdatedAt
                        FROM dbo.Notes
                        WHERE UserId = @UserId
                          AND (@Search IS NULL OR Title LIKE '%' + @Search + '%' OR Content LIKE '%' + @Search + '%')
                        ORDER BY {normalizedSortBy} {normalizedSortDir}, Id DESC
                        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
                        """;

        var totalCount = await dbConnection.ExecuteScalarAsync<int>(countSql, new { UserId = userId, Search = search });
        var notes = await dbConnection.QueryAsync<Note>(notesSql, new { UserId = userId, Search = search, Offset = offset, PageSize = pageSize });
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResult<Note>
        {
            Items = notes.ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages
        };
    }

    public async Task<Note?> GetByIdAsync(int userId, int noteId)
    {
        const string sql = """
                           SELECT Id, UserId, Title, Content, CreatedAt, UpdatedAt
                           FROM dbo.Notes
                           WHERE UserId = @UserId AND Id = @Id;
                           """;

        return await dbConnection.QueryFirstOrDefaultAsync<Note>(sql, new { UserId = userId, Id = noteId });
    }

    public async Task<int> CreateAsync(Note note)
    {
        const string sql = """
                           INSERT INTO dbo.Notes (UserId, Title, Content, CreatedAt, UpdatedAt)
                           VALUES (@UserId, @Title, @Content, @CreatedAt, @UpdatedAt);
                           SELECT CAST(SCOPE_IDENTITY() AS INT);
                           """;

        return await dbConnection.ExecuteScalarAsync<int>(sql, note);
    }

    public async Task<bool> UpdateAsync(Note note)
    {
        const string sql = """
                           UPDATE dbo.Notes
                           SET Title = @Title,
                               Content = @Content,
                               UpdatedAt = @UpdatedAt
                           WHERE Id = @Id AND UserId = @UserId;
                           """;

        var affectedRows = await dbConnection.ExecuteAsync(sql, note);
        return affectedRows == 1;
    }

    public async Task<bool> DeleteAsync(int userId, int noteId)
    {
        const string sql = """
                           DELETE FROM dbo.Notes
                           WHERE Id = @Id AND UserId = @UserId;
                           """;

        var affectedRows = await dbConnection.ExecuteAsync(sql, new { Id = noteId, UserId = userId });
        return affectedRows == 1;
    }

    private static string NormalizeSortBy(string? sortBy)
    {
        return sortBy?.ToLowerInvariant() switch
        {
            "title" => "Title",
            "createdat" => "CreatedAt",
            _ => "UpdatedAt"
        };
    }

    private static string NormalizeSortDir(string? sortDir)
    {
        return sortDir?.ToLowerInvariant() == "asc" ? "ASC" : "DESC";
    }
}
