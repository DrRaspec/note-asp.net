using Microsoft.AspNetCore.Identity;
using NotesApi.Models;

namespace NotesApi.Services;

public sealed class PasswordService : IPasswordService
{
    private readonly PasswordHasher<User> _hasher = new();

    public string HashPassword(User user, string password)
    {
        return _hasher.HashPassword(user, password);
    }

    public bool VerifyPassword(User user, string hash, string providedPassword)
    {
        var result = _hasher.VerifyHashedPassword(user, hash, providedPassword);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
