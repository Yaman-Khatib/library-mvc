namespace Library.Web.Models.Borrowings;

public sealed class BorrowingRowViewModel
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public required string BookTitle { get; set; }
    public DateTime BorrowDateUtc { get; set; }
    public DateTime DueDateUtc { get; set; }
    public DateTime? ReturnDateUtc { get; set; }

    public bool IsReturned => ReturnDateUtc.HasValue;
    public bool IsOverdue => !IsReturned && DueDateUtc < DateTime.UtcNow;
}

