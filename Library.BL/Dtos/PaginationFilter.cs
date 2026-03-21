namespace Library.BL.Dtos;

public abstract class PaginationFilter
{
    public int Page { get; init; } = 1;
    public int ItemsPerPage { get; init; } = 10;
}

