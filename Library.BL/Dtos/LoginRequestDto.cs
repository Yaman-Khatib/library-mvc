namespace Library.BL.Dtos;

public sealed class LoginRequestDto
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}
