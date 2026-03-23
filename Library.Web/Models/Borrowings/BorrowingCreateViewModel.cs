using System.ComponentModel.DataAnnotations;

namespace Library.Web.Models.Borrowings;

public sealed class BorrowingCreateViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Book is required.")]
    public int BookId { get; set; }

    public string? BookTitle { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime DueDateUtc { get; set; } = DateTime.UtcNow.AddDays(30).Date;
}
