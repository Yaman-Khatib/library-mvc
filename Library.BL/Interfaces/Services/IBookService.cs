using Library.BL.Dtos;
using Library.BL.Entities;
using Library.BL.Results;

namespace Library.BL.Interfaces.Services;

public interface IBookService
{
    Task<Result<IReadOnlyList<BookSearchRow>>> SearchAsync(BookSearchDto search, CancellationToken cancellationToken = default);
    Task<Result<Book>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<BookDetailsDto>> GetDetailsByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<LookupItemDto>>> GetGenresAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<LookupItemDto>>> GetLanguagesAsync(CancellationToken cancellationToken = default);
    Task<Result<int>> AddAsync(Book book, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(Book book, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
