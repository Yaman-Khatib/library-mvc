namespace Library.BL.Interfaces.Repositories.Models;

public readonly record struct BorrowAttempt(BorrowAttemptStatus Status, int? BorrowingId);

