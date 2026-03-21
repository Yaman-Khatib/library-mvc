namespace Library.BL.Entities;

public sealed class Borrowing
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public int BookId { get; init; }
    public DateTime BorrowDateUtc { get; init; }
    public DateTime DueDateUtc { get; init; }
    public DateTime? ReturnDateUtc { get; init; }
}

