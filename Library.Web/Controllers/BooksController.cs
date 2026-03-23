using System.Security.Claims;
using Library.BL.Dtos;
using Library.BL.Entities;
using Library.BL.Interfaces.Services;
using Library.Web.Mappings;
using Library.Web.Models.Books;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Library.Web.Controllers;

public class BooksController : Controller
{
    private readonly IBookService _books;
    private const int DefaultItemsPerPage = 8;

    public BooksController(IBookService books)
    {
        _books = books;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(string? search, string? language, string? author, int page = 1, CancellationToken cancellationToken = default)
    {
        var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
        var isAdmin = userRole == "Admin";

        var languagesResult = await _books.GetLanguagesAsync(cancellationToken);
        var authorsResult = await _books.GetAuthorsAsync(cancellationToken);

        var languageNames = languagesResult.IsSuccess && languagesResult.Value is not null
            ? languagesResult.Value.Select(l => l.Name).ToList()
            : new List<string>();

        var authors = authorsResult.IsSuccess && authorsResult.Value is not null
            ? authorsResult.Value.ToList()
            : new List<string>();

        if (page <= 0)
        {
            page = 1;
        }

        var dto = new BookSearchDto
        {
            Title = search,
            Author = author,
            LanguageName = language,
            Page = page,
            ItemsPerPage = DefaultItemsPerPage,
        };

        var totalCountResult = await _books.CountAsync(dto, cancellationToken);
        var totalItems = totalCountResult.IsSuccess ? totalCountResult.Value : 0;
        var totalPages = totalItems <= 0 ? 1 : (int)Math.Ceiling(totalItems / (double)DefaultItemsPerPage);

        if (page > totalPages)
        {
            page = totalPages;
            dto = new BookSearchDto
            {
                Title = search,
                Author = author,
                LanguageName = language,
                Page = page,
                ItemsPerPage = DefaultItemsPerPage,
            };
        }

        var result = await _books.SearchAsync(dto, cancellationToken);
        
        if (!result.IsSuccess)
        {
            return View(new BooksListViewModel 
            { 
                Books = new List<BookDisplayViewModel>(),
                IsAuthenticated = isAuthenticated,
                IsAdmin = isAdmin,
                SearchQuery = search,
                SelectedLanguage = language,
                SelectedAuthor = author,
                Languages = languageNames,
                Authors = authors,
                Page = page,
                ItemsPerPage = DefaultItemsPerPage,
                TotalItems = totalItems,
                TotalPages = totalPages,
            });
        }

        var displayBooks = (result.Value ?? new List<BookSearchRow>())
            .Select(b => b.ToDisplayViewModel())
            .ToList();

        var model = new BooksListViewModel
        {
            Books = displayBooks,
            IsAuthenticated = isAuthenticated,
            IsAdmin = isAdmin,
            SearchQuery = search,
            SelectedLanguage = language,
            SelectedAuthor = author,
            Languages = languageNames,
            Authors = authors,
            Page = page,
            ItemsPerPage = DefaultItemsPerPage,
            TotalItems = totalItems,
            TotalPages = totalPages,
        };

        return View(model);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var result = await _books.GetDetailsByIdAsync(id);
        if (!result.IsSuccess) return NotFound();
        return View(result.Value!.ToDetailsViewModel());
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        await PopulateSelectListsAsync();
        return View(new BookFormViewModel());
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookFormViewModel model, CancellationToken cancellationToken)
    {
        model.AvailableCopies = model.TotalCopies;

        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync();
            return View(model);
        }

        var result = await _books.AddAsync(model.ToNewEntity(), cancellationToken);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Could not create the book.");
            await PopulateSelectListsAsync();
            return View(model);
        }
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var result = await _books.GetByIdAsync(id);
        if (!result.IsSuccess) return NotFound();
        await PopulateSelectListsAsync();
        return View(result.Value!.ToFormViewModel());
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BookFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync();
            return View(model);
        }

        var result = await _books.UpdateAsync(model.ToExistingEntity(), cancellationToken);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Could not update the book.");
            await PopulateSelectListsAsync();
            return View(model);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _books.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateSelectListsAsync()
    {
        var genres = await _books.GetGenresAsync();
        var languages = await _books.GetLanguagesAsync();
        ViewBag.Genres = genres.IsSuccess
            ? new SelectList(genres.Value, "Id", "Name")
            : new SelectList(Enumerable.Empty<object>());
        ViewBag.Languages = languages.IsSuccess
            ? new SelectList(languages.Value, "Id", "Name")
            : new SelectList(Enumerable.Empty<object>());
    }
}
