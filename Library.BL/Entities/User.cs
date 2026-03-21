namespace Library.BL.Entities;

public sealed class User
{
    public int Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public DateTime DateOfBirth { get; init; }
    public UserRole Role { get; init; }
    public DateTime CreatedAt { get; init; }
}

