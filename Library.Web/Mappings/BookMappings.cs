using Library.BL.Dtos;
using Library.BL.Entities;
using Library.Web.Models.Books;

namespace Library.Web.Mappings;

public static class BookMappings
{
    public static BookDisplayViewModel ToDisplayViewModel(this BookSearchRow row)
    {
        return new BookDisplayViewModel
        {
            Id = row.Id,
            Title = row.Title,
            Author = row.Author,
            Genre = row.GenreName,
            Language = row.LanguageName,
            TotalCopies = row.TotalCopies,
            AvailableCopies = row.AvailableCopies,
            Description = row.Description,
        };
    }

    public static BookDetailsViewModel ToDetailsViewModel(this BookDetailsDto dto)
    {
        return new BookDetailsViewModel
        {
            Id = dto.Id,
            Title = dto.Title,
            Author = dto.Author,
            Isbn = dto.Isbn,
            TotalCopies = dto.TotalCopies,
            AvailableCopies = dto.AvailableCopies,
            Description = dto.Description,
            Language = dto.LanguageName,
            Genre = dto.GenreName,
        };
    }

    public static BookFormViewModel ToFormViewModel(this Book book)
    {
        return new BookFormViewModel
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            Isbn = book.Isbn,
            TotalCopies = book.TotalCopies,
            AvailableCopies = book.AvailableCopies,
            Description = book.Description,
            LanguageId = book.LanguageId,
            GenreId = book.GenreId,
        };
    }

    public static Book ToNewEntity(this BookFormViewModel model)
    {
        return new Book
        {
            Id = 0,
            Title = model.Title,
            Author = model.Author,
            Isbn = model.Isbn,
            TotalCopies = model.TotalCopies,
            AvailableCopies = model.TotalCopies,
            Description = model.Description,
            LanguageId = model.LanguageId,
            GenreId = model.GenreId,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public static Book ToExistingEntity(this BookFormViewModel model)
    {
        return new Book
        {
            Id = model.Id,
            Title = model.Title,
            Author = model.Author,
            Isbn = model.Isbn,
            TotalCopies = model.TotalCopies,
            AvailableCopies = model.AvailableCopies,
            Description = model.Description,
            LanguageId = model.LanguageId,
            GenreId = model.GenreId,
            CreatedAt = DateTime.UtcNow,
        };
    }
}

