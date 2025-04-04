using LibraryAPI.Data;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LibraryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookLoansController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public BookLoansController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: api/BookLoans
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookLoan>>> GetMyBookLoans()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        return await _context.BookLoans
            .Include(l => l.Book)
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.CheckoutDate)
            .ToListAsync();
    }

    // GET: api/BookLoans/All
    [HttpGet("All")]
    [Authorize(Roles = "Librarian")]
    public async Task<ActionResult<IEnumerable<BookLoan>>> GetAllBookLoans()
    {
        return await _context.BookLoans
            .Include(l => l.Book)
            .Include(l => l.User)
            .OrderByDescending(l => l.CheckoutDate)
            .ToListAsync();
    }

    // POST: api/BookLoans/checkout/5
    [HttpPost("checkout/{bookId}")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<BookLoan>> CheckoutBook(int bookId)
    {
        var book = await _context.Books.FindAsync(bookId);
        if (book == null)
        {
            return NotFound("Book not found");
        }

        if (!book.IsAvailable)
        {
            return BadRequest("Book is not available for checkout");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Create new book loan
        var bookLoan = new BookLoan
        {
            BookId = bookId,
            UserId = userId,
            CheckoutDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(5)
        };

        // Update book availability
        book.IsAvailable = false;

        _context.BookLoans.Add(bookLoan);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMyBookLoans), bookLoan);
    }

    // PUT: api/BookLoans/return/5
    [HttpPut("return/{loanId}")]
    [Authorize(Roles = "Librarian")]
    public async Task<IActionResult> ReturnBook(int loanId)
    {
        var bookLoan = await _context.BookLoans
            .Include(l => l.Book)
            .FirstOrDefaultAsync(l => l.Id == loanId);

        if (bookLoan == null)
        {
            return NotFound("Book loan not found");
        }

        if (bookLoan.ReturnDate.HasValue)
        {
            return BadRequest("Book has already been returned");
        }

        // Update loan with return date
        bookLoan.ReturnDate = DateTime.UtcNow;
        
        // Update book availability
        bookLoan.Book.IsAvailable = true;

        await _context.SaveChangesAsync();

        return NoContent();
    }
}