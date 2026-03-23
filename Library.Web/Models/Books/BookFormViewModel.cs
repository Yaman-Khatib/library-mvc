using System.ComponentModel.DataAnnotations;

namespace Library.Web.Models.Books;

public sealed class BookFormViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Author { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Isbn { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int TotalCopies { get; set; }

    [Range(0, int.MaxValue)]
    public int AvailableCopies { get; set; }

    public string? Description { get; set; }

    [Range(1, int.MaxValue)]
    public int LanguageId { get; set; }

    [Range(1, int.MaxValue)]
    public int GenreId { get; set; }
}

