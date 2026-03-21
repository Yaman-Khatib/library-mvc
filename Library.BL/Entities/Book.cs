namespace Library.BL.Entities;

public sealed class Book
{
    public int Id { get; init; }
    public required string Title { get; init; }
    public required string Author { get; init; }
    public required string Isbn { get; init; }
    public int TotalCopies { get; init; }
    public int AvailableCopies { get; init; }
    public string? Description { get; init; }
    public int LanguageId { get; init; }
    public int GenreId { get; init; }
    public DateTime CreatedAt { get; init; }
}

