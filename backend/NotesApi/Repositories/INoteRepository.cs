using NotesApi.Contracts;
using NotesApi.Models;

namespace NotesApi.Repositories;

public interface INoteRepository
{
    Task<PagedResult<Note>> GetPageAsync(int userId, string? search, string? sortBy, string? sortDir, int page, int pageSize);
    Task<Note?> GetByIdAsync(int userId, int noteId);
    Task<int> CreateAsync(Note note);
    Task<bool> UpdateAsync(Note note);
    Task<bool> DeleteAsync(int userId, int noteId);
}
