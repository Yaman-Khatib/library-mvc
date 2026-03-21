namespace Library.BL.Dtos;

public sealed class BorrowBookRequestDto
{
    public int UserId { get; init; }
    public int BookId { get; init; }
    public DateTime DueDateUtc { get; init; }
}

