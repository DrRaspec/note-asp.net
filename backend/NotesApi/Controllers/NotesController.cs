using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotesApi.Contracts;
using NotesApi.Contracts.Notes;
using NotesApi.Models;
using NotesApi.Repositories;

namespace NotesApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class NotesController(INoteRepository noteRepository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<NoteResponse>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDir,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1)
        {
            page = 1;
        }
        pageSize = Math.Clamp(pageSize, 1, 100);

        var userId = GetCurrentUserId();
        var notesPage = await noteRepository.GetPageAsync(userId, search?.Trim(), sortBy, sortDir, page, pageSize);

        return Ok(new PagedResult<NoteResponse>
        {
            Items = notesPage.Items.Select(ToResponse).ToList(),
            TotalCount = notesPage.TotalCount,
            Page = notesPage.Page,
            PageSize = notesPage.PageSize,
            TotalPages = notesPage.TotalPages
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<NoteResponse>> GetById(int id)
    {
        var userId = GetCurrentUserId();
        var note = await noteRepository.GetByIdAsync(userId, id);
        if (note is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(note));
    }

    [HttpPost]
    public async Task<ActionResult<NoteResponse>> Create([FromBody] CreateNoteRequest request)
    {
        var now = DateTime.UtcNow;
        var userId = GetCurrentUserId();

        var note = new Note
        {
            UserId = userId,
            Title = request.Title.Trim(),
            Content = request.Content?.Trim() ?? string.Empty,
            CreatedAt = now,
            UpdatedAt = now
        };

        note.Id = await noteRepository.CreateAsync(note);

        return CreatedAtAction(nameof(GetById), new { id = note.Id }, ToResponse(note));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<NoteResponse>> Update(int id, [FromBody] UpdateNoteRequest request)
    {
        var userId = GetCurrentUserId();
        var existing = await noteRepository.GetByIdAsync(userId, id);
        if (existing is null)
        {
            return NotFound();
        }

        existing.Title = request.Title.Trim();
        existing.Content = request.Content?.Trim() ?? string.Empty;
        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await noteRepository.UpdateAsync(existing);
        if (!updated)
        {
            return NotFound();
        }

        return Ok(ToResponse(existing));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();
        var deleted = await noteRepository.DeleteAsync(userId, id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    private int GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId)
            ? userId
            : throw new UnauthorizedAccessException("Missing user id claim.");
    }

    private static NoteResponse ToResponse(Note note)
    {
        return new NoteResponse
        {
            Id = note.Id,
            Title = note.Title,
            Content = note.Content,
            CreatedAt = note.CreatedAt,
            UpdatedAt = note.UpdatedAt
        };
    }
}
