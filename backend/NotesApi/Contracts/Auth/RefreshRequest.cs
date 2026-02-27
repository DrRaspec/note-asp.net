using System.ComponentModel.DataAnnotations;

namespace NotesApi.Contracts.Auth;

public sealed class RefreshRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
