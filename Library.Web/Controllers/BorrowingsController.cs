using System.Security.Claims;
using Library.BL.Dtos;
using Library.BL.Interfaces.Services;
using Library.Web.Models.Borrowings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Library.Web.Controllers;

[Authorize]
public sealed class BorrowingsController : Controller
{
    private readonly IBorrowingService _borrowings;
    private readonly IBookService _books;

    public BorrowingsController(IBorrowingService borrowings, IBookService books)
    {
        _borrowings = borrowings;
        _books = books;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Challenge();
        }

        var result = await _borrowings.ListByUserAsync(userId, onlyActive: false, cancellationToken);
        var rows = result.IsSuccess && result.Value is not null
            ? result.Value
            : Array.Empty<BorrowingDetailsDto>();

        var mapped = rows
            .Select(r => new BorrowingRowViewModel
            {
                Id = r.Id,
                BookId = r.BookId,
                BookTitle = r.BookTitle,
                BorrowDateUtc = r.BorrowDateUtc,
                DueDateUtc = r.DueDateUtc,
                ReturnDateUtc = r.ReturnDateUtc,
            })
            .ToList();

        var active = mapped.Where(x => !x.IsReturned).OrderByDescending(x => x.BorrowDateUtc).ToList();
        var returned = mapped.Where(x => x.IsReturned).OrderByDescending(x => x.ReturnDateUtc).ToList();

        return View(new BorrowingsListViewModel
        {
            Active = active,
            Returned = returned,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Return(int id, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Challenge();
        }

        var borrowingResult = await _borrowings.GetByIdAsync(id, cancellationToken);
        if (!borrowingResult.IsSuccess || borrowingResult.Value is null)
        {
            TempData["Error"] = borrowingResult.ErrorMessage ?? "Borrowing not found.";
            return RedirectToAction(nameof(Index));
        }

        if (borrowingResult.Value.UserId != userId)
        {
            return Forbid();
        }

        var result = await _borrowings.ReturnAsync(new ReturnBookRequestDto { BorrowingId = id }, cancellationToken);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage ?? "Could not return this borrowing.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = "Book returned successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? bookId, CancellationToken cancellationToken)
    {
        if (!bookId.HasValue || bookId.Value <= 0)
        {
            return RedirectToAction("Index", "Books");
        }

        var detailsResult = await _books.GetDetailsByIdAsync(bookId.Value, cancellationToken);
        if (!detailsResult.IsSuccess || detailsResult.Value is null)
        {
            TempData["Error"] = detailsResult.ErrorMessage ?? "Book not found.";
            return RedirectToAction("Index", "Books");
        }

        if (!detailsResult.Value.IsAvailable)
        {
            TempData["Error"] = "This book is not available right now.";
            return RedirectToAction("Details", "Books", new { id = bookId.Value });
        }

        return View(new BorrowingCreateViewModel
        {
            BookId = detailsResult.Value.Id,
            BookTitle = detailsResult.Value.Title,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BorrowingCreateViewModel model, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Challenge();
        }

        await PopulateBookTitleAsync(model, cancellationToken);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var dueDateUtc = model.DueDateUtc.Date;
        dueDateUtc = DateTime.SpecifyKind(dueDateUtc, DateTimeKind.Utc);

        var result = await _borrowings.BorrowAsync(new BorrowBookRequestDto
        {
            UserId = userId,
            BookId = model.BookId,
            DueDateUtc = dueDateUtc,
        }, cancellationToken);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Could not borrow this book.");
            return View(model);
        }

        TempData["Success"] = "Book borrowed successfully.";
        return RedirectToAction(nameof(Index));
    }

    private bool TryGetUserId(out int userId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdClaim, out userId) && userId > 0;
    }

    private async Task PopulateBookTitleAsync(BorrowingCreateViewModel model, CancellationToken cancellationToken)
    {
        if (model.BookId <= 0)
        {
            model.BookTitle = null;
            return;
        }

        var detailsResult = await _books.GetDetailsByIdAsync(model.BookId, cancellationToken);
        model.BookTitle = detailsResult.IsSuccess && detailsResult.Value is not null
            ? detailsResult.Value.Title
            : null;
    }
}
