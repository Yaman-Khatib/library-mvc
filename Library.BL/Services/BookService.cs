using Library.BL.Dtos;
using Library.BL.Entities;
using Library.BL.Interfaces.Repositories;
using Library.BL.Interfaces.Services;
using Library.BL.Results;

namespace Library.BL.Services;

public sealed class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;

    public BookService(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<Result<IReadOnlyList<BookSearchRow>>> SearchAsync(
        BookSearchDto search,
        CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeSearch(search);
        var rows = await _bookRepository.SearchAsync(normalized, cancellationToken);
        return Result<IReadOnlyList<BookSearchRow>>.Success(rows);
    }

    public async Task<Result<Book>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return Result<Book>.Fail(ErrorCodes.ValidationError, "Id is required.");
        }

        var book = await _bookRepository.GetByIdAsync(id, cancellationToken);
        if (book is null)
        {
            return Result<Book>.Fail(ErrorCodes.NotFound, "Book not found.");
        }

        return Result<Book>.Success(book);
    }

    public async Task<Result<int>> AddAsync(Book book, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeBook(book);
        var validationError = ValidateBook(normalized, isUpdate: false);
        if (validationError is not null)
        {
            return Result<int>.Fail(ErrorCodes.ValidationError, validationError);
        }

        var id = await _bookRepository.AddAsync(normalized, cancellationToken);
        return Result<int>.Success(id);
    }

    public async Task<Result> UpdateAsync(Book book, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeBook(book);
        var validationError = ValidateBook(normalized, isUpdate: true);
        if (validationError is not null)
        {
            return Result.Fail(ErrorCodes.ValidationError, validationError);
        }

        var updated = await _bookRepository.UpdateAsync(normalized, cancellationToken);
        if (!updated)
        {
            return Result.Fail(ErrorCodes.NotFound, "Book not found.");
        }

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return Result.Fail(ErrorCodes.ValidationError, "Id is required.");
        }

        var deleted = await _bookRepository.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return Result.Fail(ErrorCodes.NotFound, "Book not found.");
        }

        return Result.Success();
    }

    private static BookSearchDto NormalizeSearch(BookSearchDto search)
    {
        var title = NormalizeText(search.Title, 200);
        var author = NormalizeText(search.Author, 200);
        var isbn = NormalizeText(search.Isbn, 20);
        var page = search.Page <= 0 ? 1 : search.Page;
        var itemsPerPage = search.ItemsPerPage <= 0 ? 10 : search.ItemsPerPage;
        if (itemsPerPage > 100)
        {
            itemsPerPage = 100;
        }

        return new BookSearchDto
        {
            Title = title,
            Author = author,
            Isbn = isbn,
            Page = page,
            ItemsPerPage = itemsPerPage,
        };
    }

    private static string? NormalizeText(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            return trimmed[..maxLength];
        }

        return trimmed;
    }

    private static Book NormalizeBook(Book book)
    {
        return new Book
        {
            Id = book.Id,
            Title = NormalizeText(book.Title, 200) ?? string.Empty,
            Author = NormalizeText(book.Author, 200) ?? string.Empty,
            Isbn = NormalizeText(book.Isbn, 20) ?? string.Empty,
            TotalCopies = book.TotalCopies,
            AvailableCopies = book.AvailableCopies,
            Description = NormalizeText(book.Description, maxLength: int.MaxValue),
            LanguageId = book.LanguageId,
            GenreId = book.GenreId,
            CreatedAt = book.CreatedAt,
        };
    }

    private static string? ValidateBook(Book book, bool isUpdate)
    {
        if (isUpdate)
        {
            if (book.Id <= 0)
            {
                return "Id is required.";
            }
        }
        else
        {
            if (book.Id != 0)
            {
                return "Id must be 0 when creating a book.";
            }
        }

        if (string.IsNullOrWhiteSpace(book.Title))
        {
            return "Title is required.";
        }

        if (string.IsNullOrWhiteSpace(book.Author))
        {
            return "Author is required.";
        }

        if (string.IsNullOrWhiteSpace(book.Isbn))
        {
            return "ISBN is required.";
        }

        if (book.TotalCopies < 0)
        {
            return "TotalCopies must be >= 0.";
        }

        if (book.AvailableCopies < 0)
        {
            return "AvailableCopies must be >= 0.";
        }

        if (book.AvailableCopies > book.TotalCopies)
        {
            return "AvailableCopies must be <= TotalCopies.";
        }

        if (book.LanguageId <= 0)
        {
            return "LanguageId is required.";
        }

        if (book.GenreId <= 0)
        {
            return "GenreId is required.";
        }

        return null;
    }
}
