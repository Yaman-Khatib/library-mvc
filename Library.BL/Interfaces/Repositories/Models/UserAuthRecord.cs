using Library.BL.Entities;

namespace Library.BL.Interfaces.Repositories.Models;

public sealed class UserAuthRecord
{
    public int Id { get; init; }
    public required string Email { get; init; }
    public required string PasswordHash { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public UserRole Role { get; init; }
}
