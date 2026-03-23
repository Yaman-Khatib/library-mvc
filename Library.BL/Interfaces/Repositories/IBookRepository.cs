using Library.BL.Dtos;
using Library.BL.Entities;

namespace Library.BL.Interfaces.Repositories;

public interface IBookRepository
{
    Task<IReadOnlyList<BookSearchRow>> SearchAsync(BookSearchDto? search = null, CancellationToken cancellationToken = default);
    Task<int> CountAsync(BookSearchDto? search = null, CancellationToken cancellationToken = default);
    Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<BookDetailsDto?> GetDetailsByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> ListAuthorsAsync(CancellationToken cancellationToken = default);
    Task<int> AddAsync(Book book, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Book book, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
