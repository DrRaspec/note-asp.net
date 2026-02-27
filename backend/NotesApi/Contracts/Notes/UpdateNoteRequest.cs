using System.ComponentModel.DataAnnotations;

namespace NotesApi.Contracts.Notes;

public sealed class UpdateNoteRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }
}
