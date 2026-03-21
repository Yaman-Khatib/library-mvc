using Library.BL.Entities;

namespace Library.BL.Dtos;

public sealed class RegisterRequestDto
{
    public required string Email { get; init; }
    public required string Password { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public DateTime DateOfBirth { get; init; }
    public UserRole Role { get; init; } = UserRole.User;
}
