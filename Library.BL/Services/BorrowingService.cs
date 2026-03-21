using Library.BL.Dtos;
using Library.BL.Interfaces.Repositories;
using Library.BL.Interfaces.Repositories.Models;
using Library.BL.Interfaces.Services;
using Library.BL.Results;

namespace Library.BL.Services;

public sealed class BorrowingService : IBorrowingService
{
    private readonly IBorrowingRepository _borrowingRepository;

    public BorrowingService(IBorrowingRepository borrowingRepository)
    {
        _borrowingRepository = borrowingRepository;
    }

    public async Task<Result<int>> BorrowAsync(BorrowBookRequestDto request, CancellationToken cancellationToken = default)
    {
        var validationError = ValidateBorrowRequest(request);
        if (validationError is not null)
        {
            return Result<int>.Fail(ErrorCodes.ValidationError, validationError);
        }

        var attempt = await _borrowingRepository.TryBorrowAsync(request, cancellationToken);
        return attempt.Status switch
        {
            BorrowAttemptStatus.Success => Result<int>.Success(attempt.BorrowingId!.Value),
            BorrowAttemptStatus.NotAvailable => Result<int>.Fail(ErrorCodes.NotAvailable, "This book is currently checked out."),
            BorrowAttemptStatus.AlreadyBorrowed => Result<int>.Fail(ErrorCodes.AlreadyBorrowed, "You already have an active borrowing for this book."),
            _ => Result<int>.Fail(ErrorCodes.Conflict, "Borrow operation could not be completed."),
        };
    }

    public async Task<Result> ReturnAsync(ReturnBookRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.BorrowingId <= 0)
        {
            return Result.Fail(ErrorCodes.ValidationError, "BorrowingId is required.");
        }

        var attempt = await _borrowingRepository.TryReturnAsync(request, cancellationToken);
        return attempt.Status switch
        {
            ReturnAttemptStatus.Success => Result.Success(),
            ReturnAttemptStatus.NotFoundOrAlreadyReturned => Result.Fail(ErrorCodes.AlreadyReturned, "Borrowing was not found or is already returned."),
            _ => Result.Fail(ErrorCodes.Conflict, "Return operation could not be completed."),
        };
    }

    public async Task<Result<BorrowingDetailsDto>> GetByIdAsync(int borrowingId, CancellationToken cancellationToken = default)
    {
        if (borrowingId <= 0)
        {
            return Result<BorrowingDetailsDto>.Fail(ErrorCodes.ValidationError, "BorrowingId is required.");
        }

        var dto = await _borrowingRepository.GetByIdAsync(borrowingId, cancellationToken);
        if (dto is null)
        {
            return Result<BorrowingDetailsDto>.Fail(ErrorCodes.NotFound, "Borrowing not found.");
        }

        return Result<BorrowingDetailsDto>.Success(dto);
    }

    public async Task<Result<IReadOnlyList<BorrowingDetailsDto>>> ListByUserAsync(
        int userId,
        bool onlyActive,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
        {
            return Result<IReadOnlyList<BorrowingDetailsDto>>.Fail(ErrorCodes.ValidationError, "UserId is required.");
        }

        var rows = await _borrowingRepository.ListByUserAsync(userId, onlyActive, cancellationToken);
        return Result<IReadOnlyList<BorrowingDetailsDto>>.Success(rows);
    }

    private static string? ValidateBorrowRequest(BorrowBookRequestDto request)
    {
        if (request.UserId <= 0)
        {
            return "UserId is required.";
        }

        if (request.BookId <= 0)
        {
            return "BookId is required.";
        }

        if (request.DueDateUtc.Kind != DateTimeKind.Utc)
        {
            return "DueDateUtc must be in UTC.";
        }

        if (request.DueDateUtc <= DateTime.UtcNow)
        {
            return "DueDateUtc must be in the future.";
        }

        return null;
    }
}

