namespace Library.Web.Models.Books;

public class BookDisplayViewModel
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Author { get; set; }
    public required string Genre { get; set; }
    public required string Language { get; set; }
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
    public string? Description { get; set; }
    public string CoverImagePath { get; set; } = "/images/books/default.png";
    public bool IsAvailable => AvailableCopies > 0;
    public string Status => IsAvailable ? "AVAILABLE" : "RESERVED";
}

public class BooksListViewModel
{
    public required List<BookDisplayViewModel> Books { get; set; }
    public bool IsAuthenticated { get; set; }
    public bool IsAdmin { get; set; }
    public string? SearchQuery { get; set; }
    public string? SelectedLanguage { get; set; }
    public string? SelectedAuthor { get; set; }
    public IReadOnlyList<string> Languages { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> Authors { get; set; } = Array.Empty<string>();
    public int Page { get; set; } = 1;
    public int ItemsPerPage { get; set; } = 12;
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
}
