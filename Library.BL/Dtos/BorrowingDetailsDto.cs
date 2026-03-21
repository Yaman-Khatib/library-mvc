namespace Library.BL.Dtos;

public sealed class BorrowingDetailsDto
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public required string UserFullName { get; init; }
    public int BookId { get; init; }
    public required string BookTitle { get; init; }
    public DateTime BorrowDateUtc { get; init; }
    public DateTime DueDateUtc { get; init; }
    public DateTime? ReturnDateUtc { get; init; }
}

