using NotesApi.Models;

namespace NotesApi.Services;

public interface IPasswordService
{
    string HashPassword(User user, string password);
    bool VerifyPassword(User user, string hash, string providedPassword);
}
