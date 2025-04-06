using LibraryAPI.DTO;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBooks _bookService;

    public BooksController(IBooks bookService)
    {
        _bookService = bookService;
    }

    // GET: api/Books
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetBooks(
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool? availableOnly = null)
    {
        var books = await _bookService.GetAllBooksAsync(searchTerm, sortBy, availableOnly);
        return Ok(books);
    }

    // GET: api/Books/Featured
    [HttpGet("Featured")]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetFeaturedBooks()
    {
        var books = await _bookService.GetFeaturedBooksAsync();
        return Ok(books);
    }

    // GET: api/Books/5
    [HttpGet("{id}")]
    public async Task<ActionResult<BookDto>> GetBook(int id)
    {
        try
        {
            var book = await _bookService.GetBookByIdAsync(id);
            return Ok(book);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // POST: api/Books
    [HttpPost]
    [Authorize(Roles = "Librarian")]
    public async Task<ActionResult<BookDto>> CreateBook(BookDto bookDto)
    {
        var book = await _bookService.AddBookAsync(bookDto);
        return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
    }

    // PUT: api/Books/5
    [HttpPut("{id}")]
    [Authorize(Roles = "Librarian")]
    public async Task<IActionResult> UpdateBook(int id, BookDto bookDto)
    {
        if (id != bookDto.Id)
        {
            return BadRequest();
        }

        try
        {
            await _bookService.UpdateBookAsync(id, bookDto);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // DELETE: api/Books/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Librarian")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        try
        {
            await _bookService.DeleteBookAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}