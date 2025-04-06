using System.Security.Claims;
using LibraryAPI.Data;
using LibraryAPI.DTO;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Services;

public class BookLoanService : IBookLoans
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BookLoanService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }
 
    public async Task<BookLoan> AddBookLoanAsync(int bookId)
    {
        var book = await _context.Books.FindAsync(bookId);
        if (book == null || !book.IsAvailable)
        {
            throw new InvalidOperationException("Book is not available for checkout.");
        }

        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Create new book loan
        var bookLoan = new BookLoan
        {
            BookId = bookId,
            UserId = userId ?? throw new InvalidOperationException("User ID not found"),
            CheckoutDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(5)
        };

        // Update book availability
        book.IsAvailable = false;

        _context.BookLoans.Add(bookLoan);
        await _context.SaveChangesAsync();
        return bookLoan;
    }

    public async Task<IEnumerable<BookLoanDto>> GetAllBookLoansAsync()
    {
         var bookLoans = await _context.BookLoans
          .Include(l => l.Book)
          .Include(l => l.User)
          .OrderByDescending(l => l.CheckoutDate)
          .Select(l => new BookLoanDto
          {
              Id = l.Id,
              BookTitle = l.Book.Title,
              CheckoutDate = l.CheckoutDate,
              DueDate = l.DueDate,
              IsReturned = l.ReturnDate.HasValue,
              UserName = l.User.UserName,
              UserEmail = l.User.Email
          })
          .ToListAsync();

        return bookLoans;
    }

    public async Task<BookLoanDto> GetBookLoanByIdAsync(int id)
    {
        var bookLoan = await _context.BookLoans
            .Include(l => l.Book)
            .Include(l => l.User)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (bookLoan == null)
        {
            throw new InvalidOperationException("Book loan not found");
        }

        return new BookLoanDto
        {
            Id = bookLoan.Id,
            BookTitle = bookLoan.Book.Title,
            CheckoutDate = bookLoan.CheckoutDate,
            DueDate = bookLoan.DueDate,
            IsReturned = bookLoan.ReturnDate.HasValue,
            UserName = bookLoan.User.UserName,
            UserEmail = bookLoan.User.Email
        };
    }

    public async Task<IEnumerable<BookLoanDto>> GetMyBookLoansAsync(string userId)
    {
        var bookLoans = await _context.BookLoans
            .Include(l => l.Book)
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.CheckoutDate)
            .Select(l => new BookLoanDto
            {
                Id = l.Id,
                BookTitle = l.Book.Title,
                CheckoutDate = l.CheckoutDate,
                DueDate = l.DueDate,
                IsReturned = l.ReturnDate.HasValue
            })
            .ToListAsync();

        return bookLoans;
    }

    public async Task<bool> IsBookAvailableAsync(int bookId)
    {
        var book = await _context.Books
            .Include(b => b.BookLoans)
            .FirstOrDefaultAsync(b => b.Id == bookId);

        if (book == null)
        {
            throw new InvalidOperationException("Book not found");
        }

        return book.IsAvailable;
    }

    public async Task DeleteBookLoanAsync(int id)
    {
        var bookLoan = await _context.BookLoans
            .Include(l => l.Book)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (bookLoan == null)
        {
            throw new InvalidOperationException("Book loan not found");
        }

        if (!bookLoan.ReturnDate.HasValue)
        {
            throw new InvalidOperationException("Cannot delete an active loan. Book must be returned first.");
        }

        _context.BookLoans.Remove(bookLoan);
        await _context.SaveChangesAsync();
    }

    public async Task<BookLoanDto> UpdateBookLoanAsync(int id, BookLoanDto bookLoanDto)
    {
        var bookLoan = await _context.BookLoans
            .Include(l => l.Book)
            .Include(l => l.User)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (bookLoan == null)
        {
            throw new InvalidOperationException("Book loan not found");
        }

        if (bookLoanDto.IsReturned && !bookLoan.ReturnDate.HasValue)
        {
            bookLoan.ReturnDate = DateTime.UtcNow;
            bookLoan.Book.IsAvailable = true;
        }

        await _context.SaveChangesAsync();

        return new BookLoanDto
        {
            Id = bookLoan.Id,
            BookTitle = bookLoan.Book.Title,
            CheckoutDate = bookLoan.CheckoutDate,
            DueDate = bookLoan.DueDate,
            IsReturned = bookLoan.ReturnDate.HasValue,
            UserName = bookLoan.User.UserName,
            UserEmail = bookLoan.User.Email
        };
    }
}
