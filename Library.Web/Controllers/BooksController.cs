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

    public BooksController(IBookService books)
    {
        _books = books;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(string? search, string? language, string? author)
    {
        var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
        var isAdmin = userRole == "Admin";

        var dto = new BookSearchDto { Title = search, Author = author, LanguageName = language };
        var result = await _books.SearchAsync(dto);
        
        if (!result.IsSuccess)
        {
            return View(new BooksListViewModel 
            { 
                Books = new List<BookDisplayViewModel>(),
                IsAuthenticated = isAuthenticated,
                IsAdmin = isAdmin
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
            SelectedAuthor = author
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

    public async Task<IActionResult> Create()
    {
        await PopulateSelectListsAsync();
        return View(new BookFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync();
            return View(model);
        }

        var result = await _books.AddAsync(model.ToNewEntity(), cancellationToken);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
            await PopulateSelectListsAsync();
            return View(model);
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var result = await _books.GetByIdAsync(id);
        if (!result.IsSuccess) return NotFound();
        await PopulateSelectListsAsync();
        return View(result.Value!.ToFormViewModel());
    }

    [HttpPost]
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
            TempData["Error"] = result.ErrorMessage;
            await PopulateSelectListsAsync();
            return View(model);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
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
