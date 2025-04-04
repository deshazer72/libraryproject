using LibraryAPI.Data;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public BooksController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Books
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Book>>> GetBooks(
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool? availableOnly = null)
    {
        IQueryable<Book> query = _context.Books
            .Include(b => b.Reviews);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(b => b.Title.Contains(searchTerm) || b.Author.Contains(searchTerm));
        }

        // Apply availability filter
        if (availableOnly.HasValue && availableOnly.Value)
        {
            query = query.Where(b => b.IsAvailable);
        }

        // Apply sorting
        switch (sortBy?.ToLower())
        {
            case "title":
                query = query.OrderBy(b => b.Title);
                break;
            case "author":
                query = query.OrderBy(b => b.Author);
                break;
            case "rating":
                query = query.OrderByDescending(b => b.AverageRating);
                break;
            default:
                query = query.OrderBy(b => b.Title);
                break;
        }

        return await query.ToListAsync();
    }

    // GET: api/Books/Featured
    [HttpGet("Featured")]
    public async Task<ActionResult<IEnumerable<Book>>> GetFeaturedBooks()
    {
        var totalBooks = await _context.Books.CountAsync();
        var take = Math.Min(totalBooks, 6); // Display up to 6 random books
        
        return await _context.Books
            .Include(b => b.Reviews)
            .OrderBy(_ => Guid.NewGuid()) // Random ordering
            .Take(take)
            .ToListAsync();
    }

    // GET: api/Books/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Book>> GetBook(int id)
    {
        var book = await _context.Books
            .Include(b => b.Reviews)
            .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null)
        {
            return NotFound();
        }

        return book;
    }

    // POST: api/Books
    [HttpPost]
    [Authorize(Roles = "Librarian")]
    public async Task<ActionResult<Book>> CreateBook(Book book)
    {
        book.IsAvailable = true; // New books are always available
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
    }

    // PUT: api/Books/5
    [HttpPut("{id}")]
    [Authorize(Roles = "Librarian")]
    public async Task<IActionResult> UpdateBook(int id, Book book)
    {
        if (id != book.Id)
        {
            return BadRequest();
        }

        var existingBook = await _context.Books.FindAsync(id);
        if (existingBook == null)
        {
            return NotFound();
        }

        // Update properties
        existingBook.Title = book.Title;
        existingBook.Author = book.Author;
        existingBook.Description = book.Description;
        existingBook.CoverImageUrl = book.CoverImageUrl;
        existingBook.Publisher = book.Publisher;
        existingBook.PublicationDate = book.PublicationDate;
        existingBook.Category = book.Category;
        existingBook.ISBN = book.ISBN;
        existingBook.PageCount = book.PageCount;
        // Don't update IsAvailable here - that's handled by the loan process

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!BookExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/Books/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Librarian")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null)
        {
            return NotFound();
        }

        // Check if the book is currently loaned
        var activeLoans = await _context.BookLoans
            .AnyAsync(l => l.BookId == id && !l.ReturnDate.HasValue);

        if (activeLoans)
        {
            return BadRequest("Cannot delete a book that is currently on loan.");
        }

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool BookExists(int id)
    {
        return _context.Books.Any(e => e.Id == id);
    }
}