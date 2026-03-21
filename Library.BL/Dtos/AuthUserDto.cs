using Library.BL.Entities;

namespace Library.BL.Dtos;

public sealed class AuthUserDto
{
    public int Id { get; init; }
    public required string Email { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public UserRole Role { get; init; }
}
