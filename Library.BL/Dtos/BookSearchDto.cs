namespace Library.BL.Dtos;

public sealed class BookSearchDto : PaginationFilter
{
    public string? Title { get; init; }
    public string? Author { get; init; }
    public string? Isbn { get; init; }
}
