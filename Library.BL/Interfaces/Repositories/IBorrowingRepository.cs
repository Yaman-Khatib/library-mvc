using Library.BL.Dtos;
using Library.BL.Interfaces.Repositories.Models;

namespace Library.BL.Interfaces.Repositories;

public interface IBorrowingRepository
{
    Task<BorrowingDetailsDto?> GetByIdAsync(int borrowingId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BorrowingDetailsDto>> ListByUserAsync(int userId, bool onlyActive, CancellationToken cancellationToken = default);
    Task<BorrowAttempt> TryBorrowAsync(BorrowBookRequestDto request, CancellationToken cancellationToken = default);
    Task<ReturnAttempt> TryReturnAsync(ReturnBookRequestDto request, CancellationToken cancellationToken = default);
}

