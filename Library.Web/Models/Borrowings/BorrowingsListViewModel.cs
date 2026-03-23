namespace Library.Web.Models.Borrowings;

public sealed class BorrowingsListViewModel
{
    public IReadOnlyList<BorrowingRowViewModel> Active { get; set; } = Array.Empty<BorrowingRowViewModel>();
    public IReadOnlyList<BorrowingRowViewModel> Returned { get; set; } = Array.Empty<BorrowingRowViewModel>();

    public int ActiveBorrowings => Active.Count;
    public int ReturnedBorrowings => Returned.Count;
    public int TotalBorrowings => ActiveBorrowings + ReturnedBorrowings;
    public int OverdueBorrowings => Active.Count(b => b.IsOverdue);

    public bool HasAnyBorrowings => TotalBorrowings > 0;
}

