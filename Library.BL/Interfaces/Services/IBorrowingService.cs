using Library.BL.Dtos;
using Library.BL.Results;

namespace Library.BL.Interfaces.Services;

public interface IBorrowingService
{
    Task<Result<int>> BorrowAsync(BorrowBookRequestDto request, CancellationToken cancellationToken = default);
    Task<Result> ReturnAsync(ReturnBookRequestDto request, CancellationToken cancellationToken = default);
    Task<Result<BorrowingDetailsDto>> GetByIdAsync(int borrowingId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<BorrowingDetailsDto>>> ListByUserAsync(int userId, bool onlyActive, CancellationToken cancellationToken = default);
}

