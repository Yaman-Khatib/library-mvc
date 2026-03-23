namespace Library.Web.Models.Books;

public sealed class BookDetailsViewModel
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Author { get; set; }
    public required string Isbn { get; set; }
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
    public string? Description { get; set; }
    public required string Language { get; set; }
    public required string Genre { get; set; }
    public string CoverImagePath { get; set; } = "/images/books/default.png";
    public bool IsAvailable => AvailableCopies > 0;
}

